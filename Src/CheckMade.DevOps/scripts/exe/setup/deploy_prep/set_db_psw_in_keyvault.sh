#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../script_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Checking necessary environment variables are set..."
env_var_is_set "PG_APP_USER"
env_var_is_set "PG_APP_USER_PSW" "secret"

echo "Choose the keyvault to which the password for DB user '$PG_APP_USER' shall be saved"
KEYVAULT_NAME=$(confirm_and_select_resource "keyvault" "$KEYVAULT_NAME")

echo "Enter the key for the new secret (e.g. 'PRD-DB-PSW', no use of '_') and make sure it's the same as the one used \
in the code but without the 'ConnectionStrings:' prefix (see e.g. 'const string keyToPrdDbPsw' in main app and \ 
possibly integration test startup config):"
read -r secret_key

secret_key="ConnectionStrings--$secret_key"

echo "Now setting a new secret in keyvault with the password for the db app user..."
az keyvault secret set --vault-name "$KEYVAULT_NAME" --name "$secret_key" --value "$PG_APP_USER_PSW"

