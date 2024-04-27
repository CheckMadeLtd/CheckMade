#!/opt/homebrew/bin/bash

set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------

get_psql_host() {
  local host_env="$1"

  if [ "$host_env" == "Development" ]; then
    echo "local socket"
  elif [ "$host_env" == "CI" ]; then 
    echo "localhost"
  elif [ "$host_env" == "Production" ]; then
    echo "$COSMOSDB_HOST"
  else
    echo "Err: Unrecognised hosting environment:'${host_env}'"
    exit 1
  fi
}
