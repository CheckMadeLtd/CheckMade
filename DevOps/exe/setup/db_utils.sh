#!/usr/bin/env bash

set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------

get_psql_host() {
  local host_env="$1"

  if [ "$host_env" == "Development" ]; then
    echo "" # default is "local socket" but -h "local socket" wouldn't work so we leave it empty
  elif [ "$host_env" == "CI" ]; then 
    echo "localhost"
  elif [ "$host_env" == "Production" ] || [ "$host_env" == "Staging" ]; then
    if [ -z "$COSMOSDB_PG_HOST" ]; then
      echo "Err: COSMOSDB_PG_HOST is empty."
      exit 1
    fi
    echo "$COSMOSDB_PG_HOST"
  else
    echo "Err: Unrecognised hosting environment:'${host_env}'" >&2
    exit 1
  fi
}
