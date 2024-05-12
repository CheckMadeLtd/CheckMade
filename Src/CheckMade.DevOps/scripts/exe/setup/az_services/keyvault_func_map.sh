#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../script_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

KEYVAULT_NAME=$(confirm_and_select_resource "keyvault" "$KEYVAULT_NAME")

keyvault_id=$(az keyvault show --name "$KEYVAULT_NAME" --query id --output tsv)
echo "The id of the chosen keyvault is: $keyvault_id"

echo "To configure a keyvault, a functionapp needs to gain read access to it."
FUNCTIONAPP_NAME=$(confirm_and_select_resource "functionapp" "$FUNCTIONAPP_NAME")

role_readonly="Key Vault Secrets User"

echo "Now assigning keyvault read access rights to the selected functionapp (to both, production and staging slots)..."

prd_functionapp_id=$(az functionapp identity show --name "$FUNCTIONAPP_NAME" --query principalId --output tsv)
az role assignment create --assignee "$prd_functionapp_id" --scope "$keyvault_id" --role "$role_readonly"

stg_functionapp_id=$(az functionapp identity show --name "$FUNCTIONAPP_NAME" --slot 'staging' --query principalId --output tsv)
az role assignment create --assignee "$stg_functionapp_id" --scope "$keyvault_id" --role "$role_readonly"

