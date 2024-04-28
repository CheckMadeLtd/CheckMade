#!/opt/homebrew/bin/bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../../db_utils.sh"

# -------------------------------------------------------------------------------------------------------

# Script works across all hosting environments!

hosting_env="$1"
hosting_env_is_valid "$1"

echo "Checking necessary environment variables are set..."
env_var_is_set "PG_DB_NAME"
env_var_is_set "PG_APP_USER"
env_var_is_set "PG_APP_USER_PSW" "secret"
env_var_is_set "PG_SUPER_USER"

if [ "$hosting_env" == "Production" ] || [ "$hosting_env" == "Staging" ]; then
  env_var_is_set "COSMOSDB_HOST" # Needed in 'get_psql_host' function
fi

# Only needs to be set via Environment Vars in 'CI' because lack of interactivity there (e.g. no psw prompt possible)
if [ "$hosting_env" == "CI" ]; then
  env_var_is_set "PGPASSWORD" "secret"
fi

echo "-----------"
echo "This script assumes that a DB Cluster/Server is up and running in the environment '${hosting_env}'."

echo "-----------"
echo "Next, running SQL command to create user '${PG_APP_USER}' for db '${PG_DB_NAME}'..."
sql_create_user=\
"CREATE ROLE $PG_APP_USER WITH LOGIN PASSWORD '${PG_APP_USER_PSW}'; \
GRANT ALL PRIVILEGES ON DATABASE $PG_DB_NAME TO $PG_APP_USER;"

psql_host=$(get_psql_host "$hosting_env")

# Using -d postgres because creating a new user is a cluster-wide administrative task
if [ -z "$psql_host" ]; then # in case of env=Development
  psql -U "$PG_SUPER_USER" -d postgres -c "$sql_create_user"
else
  psql -h "$psql_host" -U "$PG_SUPER_USER" -d postgres -c "$sql_create_user"
fi

