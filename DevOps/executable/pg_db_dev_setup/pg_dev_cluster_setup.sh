#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/../global_utilities.sh

# -------------------------------------------------------------------------------------------------------

DB_CLUSTER_PATH="$HOME/MyPostgresDBs/CheckMade"
# The psql default user is the current system user (and thus also the superuser). 
DB_SUPERUSER=$(whoami)

echo "Now exporting PGDATABASE=postgres to make that the default parameter for psql's -d argument, because the \
usual default value (a database with the same name as the superuser) doesn't exist. \
FYI: The default user already is set to be the current superuser, i.e.: $DB_SUPERUSER"
export PGDATABASE=postgres

confirm_command \
"Install Postgres with brew (y/n)? Choose 'n' if Postgres Desktop App for Mac already installed!" \
"brew install postgresql"

confirm_command \
"Initialise Postgres DB Cluster for CheckMade in '${DB_CLUSTER_PATH}' with super-user '${DB_SUPERUSER}' (y/n)?" \
"initdb --pgdata=$DB_CLUSTER_PATH --auth-host=md5 --username=$DB_SUPERUSER"

echo "Confirm that postgresql.conf, section '# - Where to Log -' has '#' removed from relevant log_ lines... \
See example in the CheckMade Repo's Templates dir. This needs to be done for new Clusters. \
When done, continue with 'Enter'."
read -r

confirm_command \
"Start the database server for '${DB_CLUSTER_PATH}' (y/n)?" \
"pg_ctl -D $DB_CLUSTER_PATH start"

psql -l
