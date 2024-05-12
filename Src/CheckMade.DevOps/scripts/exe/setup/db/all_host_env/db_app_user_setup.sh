#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../../script_utils.sh"

# -------------------------------------------------------------------------------------------------------

# Script works across all hosting environments!

hosting_env="$1"
hosting_env_is_valid "$1"

echo "Checking necessary environment variables are set..."
env_var_is_set "PG_DB_NAME"
env_var_is_set "PG_APP_USER"
env_var_is_set "PG_APP_USER_PSW" "secret"
env_var_is_set "PG_SUPER_USER"
env_var_is_set "PG_SUPER_USER_PRD_PSW" "secret"

# Only needs to be set via Environment Vars in 'CI' because lack of interactivity there (e.g. no psw prompt possible)
if [ "$hosting_env" == "CI" ]; then
  env_var_is_set "PGPASSWORD" "secret"
fi

if [ "$hosting_env" == "Production" ]; then
  env_var_is_set "COSMOSDB_PG_CLUSTER_NAME"
  env_var_is_set "COSMOSDB_PG_HOST"
  # Using db = postgres because creating a new user is a cluster-wide administrative task
  full_cosmosdb_connection_string="sslmode=verify-full sslrootcert=system host=$COSMOSDB_PG_HOST port=5432 \
dbname=postgres user=$PG_SUPER_USER password=$PG_SUPER_USER_PRD_PSW"
fi

echo "-----------"
echo "This script assumes that a DB Cluster/Server is up and running in the environment '${hosting_env}'."

echo "-----------"
echo "Next, running commands to create SQL role '${PG_APP_USER}' for db '${PG_DB_NAME}' and granting it 'CONNECT' \
rights to the database..."

sql_grant_connect_command="GRANT CONNECT ON DATABASE $PG_DB_NAME TO $PG_APP_USER;"

if [ "$hosting_env" == "Production" ]; then
  # With CosmosDB, admin user 'citus' is not a super-user and can't e.g. create roles. Need to use az CLI tool!
  az cosmosdb postgres role create \
  --resource-group "$CURRENT_COMMON_RESOURCE_GROUP" \
  --cluster-name "$COSMOSDB_PG_CLUSTER_NAME" \
  --role-name "$PG_APP_USER" \
  --password "$PG_APP_USER_PSW"
  
  sql_command="$sql_grant_connect_command"
  echo "Don't forget to update the connection string and password for the production db also in Test projects' \
  local.settings.json files and in secrets.json files - they are used by Integration tests!"
else
  # For every other environment, using 'psql' with a proper 'super_user' works
  sql_command="CREATE ROLE $PG_APP_USER WITH LOGIN PASSWORD '${PG_APP_USER_PSW}'; \
  $sql_grant_connect_command"
fi

if [ "$hosting_env" != "Production" ] && [ "$hosting_env" != "Staging" ]; then
  psql_host=$(get_psql_host "$hosting_env")
fi

if [ -z "$psql_host" ]; then # usually in case of hosting_env=Development
  psql -U "$PG_SUPER_USER" -d postgres -c "$sql_command"
else
  psql "$full_cosmosdb_connection_string" -c "$sql_command"
fi
