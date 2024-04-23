#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/setup_utilities.sh

# -------------------------------------------------------------------------------------------------------

echo "Select functionapp whose publishing profile you want to save to GitHub..."
functionapp_name=$(confirm_and_select_resource "functionapp" "$functionapp_name")
pub_profile=$(az functionapp deployment list-publishing-profiles --name "$functionapp_name" --xml)

if [ -n "$pub_profile" ]; then

    # Sanitising function name for GitHub Secrets convention: UPPERCASE and Underscores only.
    sanitised_functionapp_name=$(echo "$functionapp_name" | tr '[:lower:]-' '[:upper:]_')
    secret_prefix="AZUREAPPSERVICE_PUBLISHPROFILE_"
    pub_profile_name=$secret_prefix${sanitised_functionapp_name}
    
    gh secret set "$pub_profile_name" --body "$pub_profile"
    
    echo "--------------------"
    echo "IMPORTANT: Check if the main GitHub Actions (Default) Workflow needs updates for:"
    echo "1) 'app-name' in the 'with' section of the 'functions-action' should be: ${functionapp_name}"
    echo "2) the 'publish-profile' name should be: $pub_profile_name"

else
    echo "Failure to retrieve publishing profile"
    exit 1
fi

# After introduction of staging slot, add --slot STAGING_SLOT_NAME to az command and add _STAGING suffix to secret name
