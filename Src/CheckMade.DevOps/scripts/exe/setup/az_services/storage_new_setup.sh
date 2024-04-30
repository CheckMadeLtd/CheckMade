#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Enter name for new storage account (lower-case letters and numbers ONLY; a random alphanumeric string will be \
appended to the chosen name)"
read -r new_storage_name

new_storage_name="$new_storage_name$(get_random_id)"
az storage account create --name "$new_storage_name"

STORAGE_NAME=$new_storage_name