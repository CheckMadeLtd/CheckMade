#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../script_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

STORAGE_NAME=$(confirm_and_select_resource "storage_account" "$STORAGE_NAME")

new_FUNCTIONAPP_NAME="functions-$(get_random_id)"

# os = Windows instead of Linux b/c on Linux, toggling (migration) between consumption and premium plans not supported!
# see/track: https://github.com/Azure/Azure-Functions/issues/2199
az functionapp create --name "$new_FUNCTIONAPP_NAME" --storage-account "$STORAGE_NAME" \
--https-only true \
--os-type Windows \
--consumption-plan-location uksouth \
--functions-version 4 --runtime dotnet-isolated \
--assign-identity

FUNCTIONAPP_NAME=$new_FUNCTIONAPP_NAME

# Preventing apossible error message for the next step that the "The Resource 'UKSouthLinuxDynamicPlan' was not found...
echo "Now waiting for 30 sec with further execution to give Azure time to fully register the new Resources..."
sleep 30

echo "Wait time is over, now creating staging slot..."
az functionapp deployment slot create --name "$FUNCTIONAPP_NAME" --slot 'staging'
az functionapp identity assign --name "$FUNCTIONAPP_NAME" --slot 'staging' 

# The AzureFunctionsWebHost__hostid helps avoid collisions when accessing a shared storage account 
# (i.e. only needed when the staging slot doesn't have its dedicated storage account)
echo "Staging slot created and system-id assigned. Now adding staging-slot-specific appsettings..."
az functionapp config appsettings set --name "$FUNCTIONAPP_NAME" --slot 'staging' \
--slot-settings AZURE_FUNCTIONS_ENVIRONMENT=Staging AzureFunctionsWebHost__hostid=staging-"$(get_random_id)" 

echo "Success, function '${FUNCTIONAPP_NAME}' was created with 'production' and 'staging' deployment slots."
echo "FYI - Assigned identity (production slot): \
$(az functionapp identity show --name "$FUNCTIONAPP_NAME" --query principalId --output tsv)"
echo "FYI - Assigned identity (staging slot): \
$(az functionapp identity show --name "$FUNCTIONAPP_NAME" --slot 'staging' --query principalId --output tsv)"

