#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/setup_utilities.sh

# -------------------------------------------------------------------------------------------------------

storage_name=$(confirm_and_select_resource "storage_account" "$storage_name")

echo "Enter the name for the new functionapp (can't contain '_').
A new Y1 Consumption plan will automatically be created alongside it:"
read -r new_functionapp_name

new_functionapp_name="${new_functionapp_name}-$(get_random_id)"

az functionapp create --name "$new_functionapp_name" --storage-account "$storage_name" \
--https-only true \
--os-type Linux \
--consumption-plan-location uksouth \
--functions-version 4 --runtime dotnet-isolated \
--assign-identity

functionapp_name=$new_functionapp_name

functionapp_assigned_id=$(az functionapp identity show --name "$functionapp_name" --query principalId --output tsv)
echo "Success, function '${functionapp_name}' was created (assigned identity: ${functionapp_assigned_id})"
