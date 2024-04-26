#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

keyvault_name=$(confirm_and_select_resource "keyvault" "$keyvault_name")

keyvault_id=$(az keyvault show --name "$keyvault_name" --query id --output tsv)
echo "The id of the chosen keyvault is: $keyvault_id"

echo "To configure a keyvault, a functionapp needs to gain read access to it."
functionapp_name=$(confirm_and_select_resource "functionapp" "$functionapp_name")
functionapp_assigned_id=$(az functionapp identity show --name "$functionapp_name" --query principalId --output tsv)

role_readonly="Key Vault Secrets User"
echo "Now assigning keyvault read access rights (new or existing keyvault) to the selected function..."
az role assignment create --assignee "$functionapp_assigned_id" --scope "$keyvault_id" --role "$role_readonly"
