#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

script_dir=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir/../../global_utils.sh"
source "$script_dir/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

working_dir_is_solution_root # because of usage of 'gh' below: to make sure gh operates on the right repo!

echo "Select functionapp whose publishing profile you want to save to GitHub..."
FUNCTIONAPP_NAME=$(confirm_and_select_resource "functionapp" "$FUNCTIONAPP_NAME")
pub_profile=$(az functionapp deployment list-publishing-profiles --name "$FUNCTIONAPP_NAME" --xml)

if [ -n "$pub_profile" ]; then

    # Sanitising function name for GitHub Secrets convention: UPPERCASE and Underscores only.
    sanitised_FUNCTIONAPP_NAME=$(echo "$FUNCTIONAPP_NAME" | tr '[:lower:]-' '[:upper:]_')
    secret_prefix="AZUREAPPSERVICE_PUBLISHPROFILE_"
    pub_profile_name=$secret_prefix${sanitised_FUNCTIONAPP_NAME}
    
    gh secret set "$pub_profile_name" --body "$pub_profile"
    
    echo "--------------------"
    echo "IMPORTANT: Check if the main GitHub Actions (Default) Workflow needs updates for:"
    echo "1) 'app-name' in the 'with' section of the 'functions-action' should be: ${FUNCTIONAPP_NAME}"
    echo "2) the 'publish-profile' name should be: $pub_profile_name"

else
    echo "Failure to retrieve publishing profile"
    exit 1
fi

# ToDo: After introduction of staging slot, add --slot STAGING_SLOT_NAME to az command and add _STAGING suffix to secret name
