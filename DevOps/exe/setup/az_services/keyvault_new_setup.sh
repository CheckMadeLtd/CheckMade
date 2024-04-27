#!/opt/homebrew/bin/bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Enter the name for the new keyvault."
read -r new_keyvault_name

new_keyvault_name="$new_keyvault_name-$(get_random_id)"

az keyvault create --name "$new_keyvault_name" --enable-rbac-authorization
keyvault_id=$(az keyvault show --name "$new_keyvault_name" --query id --output tsv)
KEYVAULT_NAME=$new_keyvault_name

echo "Success, keyvault '${KEYVAULT_NAME}' was created (id: ${keyvault_id})')"

echo "Now assigning keyvault read/write access rights to user..."
user_id=$(az ad signed-in-user show --query id --output tsv)
echo "user_id: $user_id"
role_readwrite="Key Vault Secrets Officer"
az role assignment create --assignee "$user_id" --scope "$keyvault_id" --role "$role_readwrite"
