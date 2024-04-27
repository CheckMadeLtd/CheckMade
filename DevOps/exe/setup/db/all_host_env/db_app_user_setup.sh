#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
script_dir=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir/../../../global_utils.sh"
source "$script_dir/../../db_utils.sh"

# -------------------------------------------------------------------------------------------------------

# Script works across all hosting environments!

hosting_env="$1"
hosting_env_is_valid "$1"

env_var_is_set "PG_DB_NAME"
env_var_is_set "PG_APP_USER"
env_var_is_set "PG_APP_USER_PSW" "secret"
env_var_is_set "PG_SUPER_USER"
env_var_is_set "PG_APP_USER"
env_var_is_set "COSMOSDB_HOST" # Needed in 'get_psql_host' function
echo "-----------"

# Only needs to be set via Environment Vars in 'CI' because lack of interactivity there (e.g. no psw prompt possible)
if [ "$hosting_env" == "CI" ]; then
  env_var_is_set "PGPASSWORD" "secret"
fi

echo "-----------"
echo "This script assumes that a DB Cluster/Server is up and running in the environment '${hosting_env}':"
psql -l
if [[ $? -ne 0 ]]; then
    echo "Failed to connect to PostgreSQL server."
    exit 1
fi

echo "-----------"
sql_create_user=\
"CREATE USER $PG_APP_USER WITH PASSWORD '${PG_APP_USER_PSW}'; \
GRANT ALL PRIVILEGES ON DATABASE $PG_DB_NAME TO $PG_APP_USER;"

psql_host=$(get_psql_host "$hosting_env")

psql -h "$psql_host" -U "$PG_SUPER_USER" -d postgres -c "$sql_create_user"
