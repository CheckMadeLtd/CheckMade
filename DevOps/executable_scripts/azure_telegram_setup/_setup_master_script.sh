#!/opt/homebrew/bin/bash

# Exit immediately if a command exits with a non-zero status (including in the middle of a pipeline).
set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "$BASH_SOURCE")
source $SCRIPT_DIR/../global_utilities.sh

# -------------------------------------------------------------------------------------------------------

# #######################################################################################################
# This meta-script serves as executable documentation for the setup of the Telegram Bot Azure environment
# from scratch. It also represents an overview / menu of config options outside of 'full setup from scratch'.
# #######################################################################################################


# --- PREPARATION ------------------------------------------------

echo "--------------------"
echo "IMPORTANT: You must launch this script from the CheckMade repo's root directory for 'gh' to refer to the "\
"correct repo!!!"
echo "--------------------"
echo "- Make sure Azure Portal account and 'CheckMade' subscription are set up"
echo "- Login to Azure CLI and set default account and location with 'azure_cli_init.sh'"
echo "- Create all (prd) bots with the Telegram BotFather and save its credentials in the ENVIRONMENT for access "\
"by the script"
echo "- Make sure your ENVIRONMENT has a 'GITHUB_TOKEN' set to a GitHub PAT that gives gh comprehensive admin access"

echo "--------------------"
confirm_command "Need to go back to do some prep steps (y/n)?" "exit 1"


# --- CONFIRM AZ SETTINGS / CREATE RESOURCE GROUP --------------------------------

echo "--------------------"
echo "Current Subscription:"
az account show --query name

echo "Current Defaults:"
az configure --list-defaults 

echo "Currently available resource groups:"
az group list --query "[*].name"
echo "--------------------"
confirm_script_launch $SCRIPT_DIR/group_new_setup.sh
echo "--------------------"

echo "Confirm if current subscriptions and defaults are correct (y/n)"
read -r defaults_correct

if [ "$defaults_correct" != "y" ]; then
    echo "Setup aborted."
    echo 'Set correct defaults with "az configure --defaults prop=value".'
    echo 'Set correct subscription with e.g. "az account set --subscription CheckMade"'
    exit 1
fi


# --- RUN SETUP OF RESOURCES FROM SCRATCH -----------------------------------------

echo "--------------------"
echo "Consider manually running selected sub-scripts if you don't set up the resource group from scratch."

basic_resource_setup_scripts=(
    "$SCRIPT_DIR/storage_new_setup.sh" # Updates vars: $storage_name
    "$SCRIPT_DIR/functionapp_new_setup.sh" # Updates vars: functionapp_name, functionapp_assigned_id
    "$SCRIPT_DIR/keyvault_new_setup.sh" # Updates vars: keyvault_id, keyvault_name
    "$SCRIPT_DIR/keyvault_config.sh" # Updates vars: keyvault_name, functionapp_name, function_assigned_id
    )

for script in "${basic_resource_setup_scripts[@]}"; do
    confirm_script_launch $script
done


# ToDo: Set up database


# --- DEPLOYMENT CONFIG -----------------------------------------

echo "--------------------"
echo "INSTRUCTION: In preperation for Continuous Deployment, now go to the Azure Web Portal | "\
"Function: '${functionapp_name}' | Configuration (menu) | General Settings (tab) | Stack settings (section) | "\
".NET Version (dropdown) and set a version and 'Save'! Continue here with 'Enter' when done."
read

confirm_script_launch publishing_profile_to_github.sh

echo "--------------------"
echo "Verify deployment section of the GitHub Actions workflow. Continue with 'Enter' when done."
read

echo "--------------------"
echo "Verify keyvault URL in Program.cs of top-level functionapp project. Continue with 'Enter' when done."
read

echo "--------------------"
echo "Now publish the functions to Azure via the main GitHub Action Workflow (necessary for the next step "\
"to retrieve the secret code of the live function). Continue with 'Enter' when done."
read

echo "--------------------"
confirm_script_launch telegram_bot_webhooks_setup.sh

