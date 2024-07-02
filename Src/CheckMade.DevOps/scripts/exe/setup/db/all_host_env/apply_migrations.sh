#!/usr/bin/env bash

set -e 
set -o pipefail

script_dir_apply_migr=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir_apply_migr/../../../script_utils.sh"

# -------------------------------------------------------------------------------------------------------
# Script works across all hosting environments!

db_hosting_env="$1"
db_hosting_env_is_valid "$1"

echo "Checking necessary environment variables are set..."
env_var_is_set "PG_SUPER_USER"
env_var_is_set "PG_DB_NAME"

if [ "$db_hosting_env" != "CI" ]; then
  echo "Apply all migrations to recreate database in environment '${db_hosting_env}' (y/n)?"
  read -r confirm_ops_setup
  if [ "$confirm_ops_setup" != "y" ]; then
    echo "Aborting"
    exit 0
  fi
fi

# Only needs to be set via Environment Vars in 'CI' because lack of interactivity there (e.g. no psw prompt possible)
if [ "$db_hosting_env" == "CI" ]; then
  env_var_is_set "PGPASSWORD" "secret"
fi

if [ "$db_hosting_env" == "Production" ]; then
  env_var_is_set "PG_SUPER_USER_PRD_PSW" "secret"
  env_var_is_set "COSMOSDB_PG_HOST"
  full_cosmosdb_connection_string="sslmode=verify-full sslrootcert=system host=$COSMOSDB_PG_HOST port=5432 \
dbname=$PG_DB_NAME user=$PG_SUPER_USER password=$PG_SUPER_USER_PRD_PSW"
fi

migrations_dir="$script_dir_apply_migr/../../../../sql/migrations"

for sql_file in $(ls $migrations_dir/*.sql | sort); do
  
  echo "Applying migration: $sql_file"
  
  if [ "$db_hosting_env" == "Development" ]; then
    psql -U "$PG_SUPER_USER" -d "$PG_DB_NAME" -f "$sql_file"
  elif [ "$db_hosting_env" == "CI" ]; then
    psql -h localhost -U "$PG_SUPER_USER" -d "$PG_DB_NAME" -f "$sql_file"
  else # Production or Staging
      psql "$full_cosmosdb_connection_string" -f "$sql_file"
  fi
  
  if [ $? -ne 0 ]; then
    echo "Error applying migration: $sql_file"
    exit 1
  fi
  
done

test_data_seeding_script="$migrations_dir/../seeding/test_setup_data.sql"

if [ "$db_hosting_env" == "CI" ]; then
  psql -h localhost -U "$PG_SUPER_USER" -d "$PG_DB_NAME" -f "$test_data_seeding_script"
fi

echo "All migrations applied successfully."  

