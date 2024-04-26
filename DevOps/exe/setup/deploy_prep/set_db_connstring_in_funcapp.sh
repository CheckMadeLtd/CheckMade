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
CST_HOST="$(az cosmosdb postgres cluster show -n "$postgres_cluster_name" \
--query "serverNames[*].fullyQualifiedDomainName" --output tsv)"
CST_DB="$PG_DB_NAME"
CST_PORT="5432"
CST_USER="$PG_APP_USER"
CST_PSW="MYSECRET" # Will be replaced with value from KeyVault
CST_OPTIONS="Ssl Mode=Require;Trust Server Certificate=true;Include Error Detail=true"

CST_COMPLETE="Server=$CST_HOST;Database=$CST_DB;Port=$CST_PORT;User Id=$CST_USER;Password=$CST_PSW;$CST_OPTIONS"
echo "$CST_COMPLETE"

echo "Enter the key for the Connection String (e.g. PrdDb):"
read -r CONNSTRING_KEY
CONNSTRING_SETTINGS="$CONNSTRING_KEY='$CST_COMPLETE'"

echo "Now setting Connection String in '${functionapp_name}'"
set -x
az webapp config connection-string set --name "$functionapp_name" --connection-string-type PostgreSQL \
--settings "$CONNSTRING_SETTINGS"
