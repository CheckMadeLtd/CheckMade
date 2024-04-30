#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

KEYVAULT_NAME=$(confirm_and_select_resource "keyvault" "$KEYVAULT_NAME")

keyvault_id=$(az keyvault show --name "$KEYVAULT_NAME" --query id --output tsv)
echo "The id of the chosen keyvault is: $keyvault_id"

echo "To configure a keyvault, a functionapp needs to gain read access to it."
FUNCTIONAPP_NAME=$(confirm_and_select_resource "functionapp" "$FUNCTIONAPP_NAME")
functionapp_assigned_id=$(az functionapp identity show --name "$FUNCTIONAPP_NAME" --query principalId --output tsv)

role_readonly="Key Vault Secrets User"
echo "Now assigning keyvault read access rights (new or existing keyvault) to the selected function..."
az role assignment create --assignee "$functionapp_assigned_id" --scope "$keyvault_id" --role "$role_readonly"
