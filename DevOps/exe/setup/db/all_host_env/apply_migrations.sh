#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
script_dir=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir/../../db_utils.sh"

# -------------------------------------------------------------------------------------------------------

# Works across all hosting environments!

hosting_env="$1"

[[ -z "$PG_DB_NAME" ]] && echo "Err: PG_DB_NAME is NOT set" && exit 1 || echo "PG_DB_NAME: $PG_DB_NAME"

psql_host=$(get_psql_host "$hosting_env")

if [ "$hosting_env" != "CI" ]; then
  echo "Apply all migrations to recreate database (y/n)?"
  read -r confirm_ops_setup
  if [ "$confirm_ops_setup" != "y" ]; then
    echo "Aborting"
    exit 0
  fi
fi

migrations_dir="$script_dir/../../../../sql/migrations"
for sql_file in $(ls $migrations_dir/*.sql | sort); do
  echo "Applying migration: $sql_file"
  psql -h "$psql_host" -U $PG_SUPER_USER -d "$PG_DB_NAME" -f "$sql_file"
  if [ $? -ne 0 ]; then
    echo "Error applying migration: $sql_file"
    exit 1
  fi
done
echo "All migrations applied successfully."  

