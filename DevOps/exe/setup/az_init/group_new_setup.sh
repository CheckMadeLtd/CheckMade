#!/opt/homebrew/bin/bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Enter name of new resource group (will be set as default) or leave empty to skip"
read -r new_resource_group

new_resource_group="$new_resource_group-$(get_random_id)"

if [ -n "$new_resource_group" ]; then
    az group create --name "$new_resource_group"
    az configure --defaults group="$new_resource_group"
    echo "Resource group '${new_resource_group}' was created and made default"
fi
