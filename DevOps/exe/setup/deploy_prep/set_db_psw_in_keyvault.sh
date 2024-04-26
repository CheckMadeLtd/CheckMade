#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

[[ -z "$PG_APP_USER" ]] && echo "Err: PG_APP_USER is NOT set" && exit 1 || echo "PG_APP_USER: $PG_APP_USER"
[[ -z "$PG_APP_USER_PSW" ]] && echo "Err: PG_APP_USER_PSW is NOT set" && exit 1 || echo "PG_APP_USER_PSW is set"

echo "Choose the keyvault to which the password for DB user '$PG_APP_USER' shall be saved"
keyvault_name=$(confirm_and_select_resource "keyvault" "$keyvault_name")

echo "Enter the key for the new secret (e.g. 'PrdDbPsw'):"
read -r SECRET_KEY

SECRET_KEY="ConnectionStrings--$SECRET_KEY"

echo "Now setting a new secret in keyvault  the password for the db app user stored in the environment into"
az keyvault secret set --vault-name "$keyvault_name" --name "$SECRET_KEY" --value "$PG_APP_USER_PSW"

