#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/../global_utilities.sh

# -------------------------------------------------------------------------------------------------------

DEV_DB_NAME="cm_ops"
DEV_DB_SETUP_SCRIPT="$SCRIPT_DIR/../../sql_scripts/dev/create_ops_db_and_user.sql"

echo "This script assumes the DEV DB Cluster is has been started. Check with 'psql -l' if in doubt."

confirm_command \
"Create operational dev database and app user (y/n)?" \
"psql -d postgres -f $DEV_DB_SETUP_SCRIPT"

echo "Apply all migrations to recreate database (y/n)?"
read -r confirm_ops_setup
if [ "$confirm_ops_setup" == "y" ]; then
  MIGRATIONS_DIR="$SCRIPT_DIR/../../sql_scripts/general/migrations"
  for sql_file in $(ls $MIGRATIONS_DIR/*.sql | sort); do
    echo "Applying migration: $sql_file"
    psql -d $DEV_DB_NAME -f "$sql_file"
    if [ $? -ne 0 ]; then
      echo "Error applying migration: $sql_file"
      exit 1
    fi
  done
  echo "All migrations applied successfully."  
fi

echo "-----"
echo "Script done. Next steps:"
echo "- In case of Rider IDE, connect to the DB via the Database Tool Window. Do NOT use the superuser! \
Instead, use the same user that the app will use and which was created in the setup script. This way, \
the database explorer will replicate the privileges the app itself will have, and we can not accidentally \
break the database outside of verified DevOps DB scripts."
echo "- Set up the application's access to the local dev DB via a connection string, using the same app user."

