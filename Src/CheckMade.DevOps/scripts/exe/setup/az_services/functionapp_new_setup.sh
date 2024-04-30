#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

STORAGE_NAME=$(confirm_and_select_resource "storage_account" "$STORAGE_NAME")

echo "Enter the name for the new functionapp (can't contain '_', and a random alphanumeric string will be appended \
to the chosen name). Also, a new Y1 Consumption plan will automatically be created alongside it:"
read -r new_FUNCTIONAPP_NAME

new_FUNCTIONAPP_NAME="${new_FUNCTIONAPP_NAME}-$(get_random_id)"

az functionapp create --name "$new_FUNCTIONAPP_NAME" --storage-account "$STORAGE_NAME" \
--https-only true \
--os-type Linux \
--consumption-plan-location uksouth \
--functions-version 4 --runtime dotnet-isolated \
--assign-identity

FUNCTIONAPP_NAME=$new_FUNCTIONAPP_NAME

# Wait for 10 seconds with further execution to give Azure time to fully register the new Resources, preventing a 
# possible error message for the next step that the "The Resource 'UKSouthLinuxDynamicPlan' was not found...
sleep 10

az functionapp deployment slot create --name "$FUNCTIONAPP_NAME" --slot 'staging'
az functionapp identity assign --name "$FUNCTIONAPP_NAME" --slot 'staging' 

echo "Success, function '${FUNCTIONAPP_NAME}' was created with 'production' and 'staging' deployment slots."
echo "FYI - Assigned identity (production slot): \
$(az functionapp identity show --name "$FUNCTIONAPP_NAME" --query principalId --output tsv)"
echo "FYI - Assigned identity (staging slot): \
$(az functionapp identity show --name "$FUNCTIONAPP_NAME" --slot 'staging' --query principalId --output tsv)"

