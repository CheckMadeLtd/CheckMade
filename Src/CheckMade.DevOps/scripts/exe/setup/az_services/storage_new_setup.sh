#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../script_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Enter name for new storage account (lower-case letters and numbers ONLY; a random alphanumeric string will be \
appended to the chosen name)"
read -r new_storage_name

new_storage_name="$new_storage_name$(get_random_id)"
az storage account create --name "$new_storage_name"
STORAGE_NAME=$new_storage_name

echo "Now creating the CheckMade Blob Container inside the new storage account..."
az storage container create --account-name "$STORAGE_NAME" --name checkmade --fail-on-exist

echo "Now adjust the BlobContainerClientUri and BlobContainerAccountName in GitHub Actions main workflow 
and continue with Enter when done:"
echo "New BlobContainerClientUri: https://$STORAGE_NAME.blob.core.windows.net/checkmade"
echo "New BlobContainerClientAccountName: $STORAGE_NAME"
echo "FYI: We will update the BlobContainerClientAccountKey in a later step of the overall setup orchestration."
read -r
