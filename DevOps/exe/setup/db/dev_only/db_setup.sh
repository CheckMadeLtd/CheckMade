#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../../global_utilities.sh"

# -------------------------------------------------------------------------------------------------------

echo "----------------------"
echo "This is for DEV environment only!! \
For production environment, db setup happens by default via Cosmos DB cluster setup. \
For CI environment it is set up in the main workflow in the 'Set up PostgreSQL DB and User' (or similar) step!"
echo "----------------------"
echo "Assuming, Postgres was already installed on this machine (e.g. 'brew install postgres@16')!"
psql --version
if [[ $? -ne 0 ]]; then
    echo "Err: PostgreSQL not installed?"
    exit 1
fi
echo "----------------------"
echo "FYI: The default psql user is set to be the current superuser, i.e.: $PG_SUPERUSER"
echo "FYI: psql on dev won't ask for a password because it uses the 'trusted' unix 'local socket'"

DB_CLUSTER_PATH="$HOME/MyPostgresDBs/CheckMade"

confirm_command \
"Initialise Postgres DB Cluster for CheckMade in '${DB_CLUSTER_PATH}' with super-user '${PG_SUPERUSER}' (y/n)?" \
"initdb --pgdata=$DB_CLUSTER_PATH --auth-host=md5 --username=$PG_SUPERUSER"

echo "Next, setting logging config for our postgres db to '${DB_CLUSTER_PATH}/log/' with rotation etc."
log_settings=('#log_destination' '#logging_collector' '#log_directory' '#log_filename' '#log_file_mode' \
'#log_rotation_age' '#log_rotation_size')
for setting in "${log_settings[@]}"; do
  # Uncomment the line with sed
  sed -i "" "/^$setting/s/^#//" "${DB_CLUSTER_PATH}/postgresql.conf"
done

confirm_command \
"Start the database server for '${DB_CLUSTER_PATH}' (y/n)?" \
"pg_ctl -D $DB_CLUSTER_PATH start"

SQL_TO_CREATE_OPS_DB="CREATE DATABASE $PG_DB_NAME;"

confirm_command \
"Create the '${PG_DB_NAME}' database now (y/n)?"
"psql -d postgres -c $SQL_TO_CREATE_OPS_DB" 

psql -l

confirm_script_launch "$SCRIPT_DIR/db_app_user_setup.sh" "Development"
confirm_script_launch "$SCRIPT_DIR/apply_migrations.sh"

echo "----------------------"
echo "Next steps:"
echo "- In case of Rider IDE, connect to the DB via the Database Tool Window. Do NOT use the superuser! \
Instead, use the same user that the app will use and which was created in the setup script. This way, \
the database explorer will replicate the privileges the app itself will have, and we can not accidentally \
break the database outside of verified DevOps DB scripts."
echo "- Set up the application's access to the local dev DB via a connection string."
