#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../script_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Choose the storage account whose key1 shall be saved in keyvault"
STORAGE_NAME=$(confirm_and_select_resource "storage_account" "$STORAGE_NAME")

echo "Choose the keyvault to which the key1 of storage shall be saved"
KEYVAULT_NAME=$(confirm_and_select_resource "keyvault" "$KEYVAULT_NAME")

echo "Now retrieving key1 from storage account..."
key1=$(az storage account keys list -n "$STORAGE_NAME" --query "[0].value" --output tsv)

echo "Now setting key1 as a new secret called 'BlobContainerClientAccountKey' in keyvault..."
az keyvault secret set --vault-name "$KEYVAULT_NAME" --name BlobContainerClientAccountKey --value "$key1"

echo "Now update the BLOB_CONTAINER_CLIENT_ACCOUNT_KEY secret stored in GitHub Repo Secrets. 
The new key has been copied to the clipboard. Continue with Enter when done."
echo $key1 | pbcopy
read -r
