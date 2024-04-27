#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

[[ -z "$PG_DB_NAME" ]] && echo "Err: PG_DB_NAME is NOT set" && exit 1 || echo "PG_DB_NAME: $PG_DB_NAME"
[[ -z "$PG_APP_USER" ]] && echo "Err: PG_APP_USER is NOT set" && exit 1 || echo "PG_APP_USER: $PG_APP_USER"

echo "Choose the functionapp to which the Connection String shall be added:"
functionapp_name=$(confirm_and_select_resource "functionapp" "$functionapp_name")

echo "Now constructing connection string from its components in ADO.NET format..."
COSMOSDB_DB="$PG_DB_NAME"
COSMOSDB_PORT="5432"
COSMOSDB_USER="$PG_APP_USER"
COSMOSDB_PSW="MYSECRET" # Will be replaced with value from KeyVault
COSMOSDB_OPTIONS="Ssl Mode=Require;Trust Server Certificate=true;Include Error Detail=true"

COSMOSDB_CONNSTRING="Server=$COSMOSDB_HOST;Database=$COSMOSDB_DB;Port=$COSMOSDB_PORT;User Id=$COSMOSDB_USER;\
Password=$COSMOSDB_PSW;$COSMOSDB_OPTIONS"
echo "$COSMOSDB_CONNSTRING"

echo "Enter the key for the Connection String (e.g. PrdDb):"
read -r CONNSTRING_KEY
CONNSTRING_SETTINGS="$CONNSTRING_KEY='$COSMOSDB_CONNSTRING'"

echo "Now setting Connection String in '${functionapp_name}'"
set -x
az webapp config connection-string set --name "$functionapp_name" --connection-string-type PostgreSQL \
--settings "$CONNSTRING_SETTINGS"
