#!/opt/homebrew/bin/bash

set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "$BASH_SOURCE")
source $SCRIPT_DIR/setup_utilities.sh

# -------------------------------------------------------------------------------------------------------

echo "Enter name for new storage account (lower-case letters and numbers ONLY!)"
read -r new_storage_name

new_storage_name="$new_storage_name$(get_random_id)"
az storage account create --name $new_storage_name

storage_name=$new_storage_name