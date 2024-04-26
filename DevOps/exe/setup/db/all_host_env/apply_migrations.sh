#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")

# -------------------------------------------------------------------------------------------------------

# Works across all hosting environments!

HOSTING_ENV="$1"

if [ "$HOSTING_ENV" != "CI" ]; then
  echo "Apply all migrations to recreate database (y/n)?"
  read -r confirm_ops_setup
  if [ "$confirm_ops_setup" != "y" ]; then
    echo "Aborting"
    exit 0
  fi
fi

MIGRATIONS_DIR="$SCRIPT_DIR/../../../../sql/migrations"
for sql_file in $(ls $MIGRATIONS_DIR/*.sql | sort); do
  echo "Applying migration: $sql_file"
  psql -d $DEV_DB_NAME -f "$sql_file"
  if [ $? -ne 0 ]; then
    echo "Error applying migration: $sql_file"
    exit 1
  fi
done
echo "All migrations applied successfully."  


# ToDo: generlaise the above psql with the following AND in beginning of script, check for presence of needed env variables.
#psql -h localhost -U $PG_SUPER_USER -d ${{ vars.POSTGRES_OPS_DB_NAME }} -f "$sql_file"