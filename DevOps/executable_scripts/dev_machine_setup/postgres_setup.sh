#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "$BASH_SOURCE")
source $SCRIPT_DIR/../global_utilities.sh

# -------------------------------------------------------------------------------------------------------

DB_PATH="$HOME/MyPostgresDBs/CheckMade"
DB_SUPERUSER=danielgorin
DB_OPS_NAME=checkmadeops

confirm_command \
"Install Postgres with brew (y/n)? Choose 'n' if Postgres Desktop App for Mac already installed!" \
"brew install postgresql"

confirm_command \
"Initialise Postgres DB Cluster for CheckMade in '${DB_PATH}' with super-user '${DB_SUPERUSER}' (y/n)?" \
"initdb --pgdata=$DB_PATH --auth-host=md5 --username=$DB_SUPERUSER"

confirm_command \
"Start the database server for '${DB_PATH}' (y/n)?" \
"pg_ctl -D $DB_PATH -l $DB_PATH/logfile.log start"

echo "-----"
echo "Next you can confirm launching the psql prompt to administrate the db cluster. Once inside the psql interactive "\
"prompt, use the \i command to read SQL scripts saved here, e.g. to set up the $DB_OPS_NAME database with credentials."
echo "-----"

confirm_command \
"Connect to default 'postgres' db in cluster '${DB_PATH}' as super-user '${DB_SUPERUSER}' for admin purpose (y/n)?" \
"psql -d postgres -U $DB_SUPERUSER"

# Check adding more details to the manual instructions below.
echo "-----"
echo "Next steps:"
echo "- Go to Rider and attempt connecting to the DB via the DB Tool Window"
echo "- Set up the application's access to the local DB via connection string."

