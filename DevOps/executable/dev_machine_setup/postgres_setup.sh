#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/../global_utilities.sh

# -------------------------------------------------------------------------------------------------------

DB_PATH="$HOME/MyPostgresDBs/CheckMade"
# The psql default user is the current system user (and thus also the superuser). 
DB_SUPERUSER=$(whoami)
OPS_DB_SETUP_SCRIPT="$SCRIPT_DIR/sql_scripts/setup_cm_ops_db.sql"

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

echo "Now exporting PGDATABASE=postgres to make that the default parameter for psql's -d argument, because the \
usual default value (a database with the same name as the superuser) doesn't exist. \
FYI: The default user already is set to be the current superuser, i.e.: $DB_SUPERUSER"
export PGDATABASE=postgres

confirm_command \
"Now execute '${OPS_DB_SETUP_SCRIPT}' with psql with default superuser $DB_SUPERUSER (y/n)?" \
"psql -f $OPS_DB_SETUP_SCRIPT"

echo "-----"
echo "Script done. Next steps:"
echo "- In case of Rider IDE, connect to the DB via the Database Tool Window. Do NOT use the superuser! \
Instead, use the same user that the app will use and which was created in the setup script. This way, \
the database explorer will replicate the privileges the app itself will have, and we can not accidentally \
break the database outside of verified DevOps DB scripts."
echo "- Set up the application's access to the local dev DB via a connection string, using the same app user."

