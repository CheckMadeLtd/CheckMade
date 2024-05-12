#!/usr/bin/env bash

set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------


confirm_script_launch() {
  local script_path="$1"
  local first_argument="$2"
  
  echo "Launch $script_path (y/n)?" >&2
  read -r confirm_launch
  if [ "$confirm_launch" == "y" ]; then
    if [ -z "$first_argument" ]; then
      source "$script_path"
    else
      source "$script_path" "$first_argument"
    fi
  fi
}

confirm_command() {
  local confirm_msg="$1"
  local command="$2"

  echo $confirm_msg
  read -r answer
  if [ "$answer" == "y" ]; then
    eval "$command"
  fi
}

get_random_id() {
  # Generates a random 5-character alphanumeric string in lower case
  openssl rand -base64 18 | tr -dc 'a-z0-9' | fold -w 5 | head -n 1
}

working_dir_is_solution_root() {
  # Check for any .sln files in the current directory
  if ! ls *.sln 1> /dev/null 2>&1; then
    echo "Err: No solution file found in the working dir. You must run this script from the solution root." >&2
    exit 1
  fi
}

hosting_env_is_valid() {
  local hosting_env="$1" 
  if [ "$hosting_env" != "Development" ] && [ "$hosting_env" != "CI" ] && \
  [ "$hosting_env" != "Staging" ] && [ "$hosting_env" != "Production" ]; then
    echo "Err: Hosing Environment '${hosting_env}' is not valid." >&2
    exit 1  
  fi
}

env_var_is_set() {
  local var_name="$1"
  local var_is_secret="$2"
  local var_value
  var_value="${!var_name}" # dereferencing from the var name into its actual value
  
  if [ -z "$var_value" ]; then
    echo "Err: $var_name is not set" >&2
    exit 1
  else
    if [ "$var_is_secret" == "secret" ]; then
      echo "$var_name is set" >&2
    else
      echo "$var_name is: $var_value"
    fi
  fi
}

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

