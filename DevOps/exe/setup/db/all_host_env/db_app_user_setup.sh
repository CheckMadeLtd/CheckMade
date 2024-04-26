#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../../global_utilities.sh"

# -------------------------------------------------------------------------------------------------------

# Works across all hosting environments!

HOSTING_ENV="$1"

if [ "$HOSTING_ENV" != "Development" ] && [ "$HOSTING_ENV" != "Production" ] && [ "$HOSTING_ENV" != "CI" ]; then
  echo "Err: The current value for HOSING_ENV ('${HOSTING_ENV}') is not valid!"
  exit 1
fi

echo "-----------"
echo "Checking necessary environment variables (which should have been set in env '${HOSTING_ENV}' \
before launching this script):"
[[ -z "$PG_DB_NAME" ]] && echo "Err: PG_DB_NAME is NOT set" && exit 1 || echo "PG_DB_NAME: $PG_DB_NAME"
[[ -z "$PG_APP_USER" ]] && echo "Err: PG_APP_USER is NOT set" && exit 1 || echo "PG_APP_USER: $PG_APP_USER"
[[ -z "$PG_APP_USER_PSW" ]] && echo "Err: PG_APP_USER_PSW is NOT set" && exit 1 || echo "PG_APP_USER_PSW is set"
[[ -z "$PG_SUPER_USER" ]] && echo "Err: PG_SUPER_USER is NOT set" && exit 1 || echo "PG_SUPER_USER: $PG_SUPER_USER"

# Only needs to be set via Environment Vars in 'CI' because lack of interactivity there (e.g. no psw prompt possible)
if [ "$HOSTING_ENV" == "CI" ]; then
  [[ -z "$PGPASSWORD" ]] && echo "Err: PGPASSWORD is NOT set" && exit 1 || echo "PGPASSWORD is set"
fi

echo "-----------"
echo "This script assumes that a DB Cluster/Server is up and running in the environment '${HOSTING_ENV}':"
psql -l
if [[ $? -ne 0 ]]; then
    echo "Failed to connect to PostgreSQL server."
    exit 1
fi

echo "-----------"
SQL_CREATE_USER=\
"CREATE USER $PG_APP_USER WITH PASSWORD '${PG_APP_USER_PSW}'; \
GRANT ALL PRIVILEGES ON DATABASE $PG_DB_NAME TO $PG_APP_USER;"

# ToDo: set 'host' depending on HOSTING ENV !! Then run psql with that host, to execute the above SQL command!
#psql -h.....