#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Enter the name for the new keyvault."
read -r new_keyvault_name

new_keyvault_name="$new_keyvault_name-$(get_random_id)"

az keyvault create --name "$new_keyvault_name" --enable-rbac-authorization
keyvault_id=$(az keyvault show --name "$new_keyvault_name" --query id --output tsv)
keyvault_name=$new_keyvault_name

echo "Success, keyvault '${keyvault_name}' was created (id: ${keyvault_id})')"

echo "Now assigning keyvault read/write access rights to user..."
user_id=$(az ad signed-in-user show --query id --output tsv)
echo "user_id: $user_id"
role_readwrite="Key Vault Secrets Officer"
az role assignment create --assignee "$user_id" --scope "$keyvault_id" --role "$role_readwrite"
