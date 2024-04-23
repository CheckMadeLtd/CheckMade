#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/../global_utilities.sh

# -------------------------------------------------------------------------------------------------------

DB_PATH="$HOME/MyPostgresDBs/CheckMade"
DB_SUPERUSER=danielgorin
OPS_DB_SETUP_SCRIPT="sql_scripts/setup_cm_ops_db.sql"

confirm_command \
"Install Postgres with brew (y/n)? Choose 'n' if Postgres Desktop App for Mac already installed!" \
"brew install postgresql"

confirm_command \
"Initialise Postgres DB Cluster for CheckMade in '${DB_PATH}' with super-user '${DB_SUPERUSER}' (y/n)?" \
"initdb --pgdata=$DB_PATH --auth-host=md5 --username=$DB_SUPERUSER"

echo "In postgresql.conf go to the section '# - Where to Log -' and remove '#' from relevant log_ lines... \
See example in the CheckMade Repo's Templates dir. When done, continue with 'Enter'."
read -r

confirm_command \
"Start the database server for '${DB_PATH}' (y/n)?" \
"pg_ctl -D $DB_PATH start"

echo "-----"
echo "Next you can confirm launching the psql prompt to administrate the db cluster. Once inside the psql interactive \
prompt, use the \i command to read SQL scripts saved here, e.g. to set up the $DB_OPS_NAME database with credentials."
echo "-----"

confirm_command \
"Now execute '${OPS_DB_SETUP_SCRIPT}' with psql (y/n)?" \
"psql -U $DB_SUPERUSER -d postgres -f $OPS_DB_SETUP_SCRIPT"


# Check adding more details to the manual instructions below.
echo "-----"
echo "Next steps:"
echo "- In case of Rider IDE, connect to the DB via the Database Tool Window"
echo "- Set up the application's access to the local dev DB via a connection string."

