#!/usr/bin/env bash

set -e 
set -o pipefail

script_dir_apply_selected_migr=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir_apply_selected_migr/../script_utils.sh"

# -------------------------------------------------------------------------------------------------------

env_var_is_set "PG_SUPER_USER_PRD"
env_var_is_set "PG_SUPER_USER_PRD_PSW" "secret"
env_var_is_set "PG_SUPER_USER_DEV"
env_var_is_set "PG_DB_NAME"
env_var_is_set "PG_APP_USER"
env_var_is_set "COSMOSDB_PG_HOST"

full_cosmosdb_connection_string="sslmode=verify-full sslrootcert=system host=$COSMOSDB_PG_HOST port=5432 \
dbname=$PG_DB_NAME user=$PG_SUPER_USER_PRD password=$PG_SUPER_USER_PRD_PSW"

declare -A MigrationFiles

migrations_dir="$script_dir_apply_selected_migr/../../sql/migrations"

[ -d "$migrations_dir" ] || { echo "Migration directory not found"; exit 1; }

# Loop through all SQL migration files
for sql_file in "$migrations_dir"/*; do
  key=$(basename "$sql_file" | cut -c 1-3) 
  MigrationFiles[$key]=$sql_file
done 

while true; do
  echo "-------------"
  echo "Enter the first three characters of the migration file you want to apply:"
  read -r migration_id
  
  sql_file=${MigrationFiles[$migration_id]}
  
  if [ -z "$sql_file" ]; then
    echo "Error: Could not resolve filename."
    exit 1
  fi
  
  echo "The full file name to be applied is: $sql_file. Do you confirm (y/n)?"
  read -r is_confirmed
  
  if [ "$is_confirmed" == "y" ]; then
    
    echo "Do you want to apply migration '${migration_id}' to the local dev db (y/n)?"
    read -r is_dev_confirmed
    
    if [ "$is_dev_confirmed" == "y" ]; then
      set -x
      psql -U "$PG_SUPER_USER_DEV" -d "$PG_DB_NAME" -f "$sql_file"
      set +x
      echo "Migration '${migration_id}' was applied to dev db."
    fi
    
    echo "Do you want to apply the migration '${migration_id}' to the prd db?"
    echo "If you are absolutely sure, enter a full 'yes':"
    read -r is_prd_confirmed
    
    if [ "$is_prd_confirmed" == "yes" ]; then
      set -x
      psql "$full_cosmosdb_connection_string" -f "$sql_file"
      set +x
      echo "Migration '${migration_id}' was applied to prd db."
    fi
  
  fi

done
