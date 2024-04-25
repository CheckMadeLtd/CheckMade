#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/../global_utilities.sh

# -------------------------------------------------------------------------------------------------------

DB_CLUSTER_PATH="$HOME/MyPostgresDBs/CheckMade"
DB_SUPERUSER=$(whoami)

echo "Assuming, Postgres is installed (e.g. with 'brew install postgres@16')!"
echo "----------------------"
echo "FYI: The default psql user is set to be the current superuser, i.e.: $DB_SUPERUSER"
echo "FYI: psql on dev won't ask for a password because it uses the 'trusted' unix 'local socket'"

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
