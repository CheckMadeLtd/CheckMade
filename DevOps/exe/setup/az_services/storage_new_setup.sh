#!/opt/homebrew/bin/bash

set -e 
set -o pipefail

script_dir=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir/../../global_utils.sh"
source "$script_dir/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Enter name for new storage account (lower-case letters and numbers ONLY!)"
read -r new_storage_name

new_storage_name="$new_storage_name$(get_random_id)"
az storage account create --name "$new_storage_name"

STORAGE_NAME=$new_storage_name