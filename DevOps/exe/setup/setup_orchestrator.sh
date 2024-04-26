#!/opt/homebrew/bin/bash

# Exit immediately if a command exits with a non-zero status (including in the middle of a pipeline).
set -e 
set -o pipefail
SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../global_utilities.sh"

# -------------------------------------------------------------------------------------------------------

# #######################################################################################################
# This meta-script serves as executable documentation for the setup of Azure/Telegram/DB from scratch. 
# It also represents an overview / menu of config options outside of 'full setup from scratch'.
# #######################################################################################################


# --- PREPARATION ------------------------------------------------

echo "--------------------"
echo "IMPORTANT: You must launch this script from the CheckMade repo's root directory for 'gh' to refer to the \
correct repo!!!"
echo "--------------------"
echo "Make sure these preparatory steps have been completed:"
echo "- Azure Portal account and 'CheckMade' subscription have been set up"
echo "- Logged in to Azure CLI and default account and location have been set (use 'az_init/az_cli_init.sh')"
echo "- All needed bots have been created with the Telegram BotFather and their credentials/tokens saved in the \
ENVIRONMENT for access by subsequent scripts"
echo "- 'GITHUB_TOKEN' in ENVIRONMENT has been set to a GitHub PAT that gives gh comprehensive admin access"

echo "--------------------"
confirm_command \
"Need to go back to do some prep steps (y/n)?" \
"exit 1"


# --- CONFIRM AZ SETTINGS / CREATE RESOURCE GROUP --------------------------------

echo "--------------------"
echo "Current Subscription:"
az account show --query name

echo "Current Defaults:"
az configure --list-defaults 

echo "Currently available resource groups:"
az group list --query "[*].name"

echo "--------------------"
confirm_script_launch "$SCRIPT_DIR/az_init/group_new_setup.sh"

echo "--------------------"
echo "Confirm if current subscriptions and defaults are correct (y/n)"
read -r defaults_correct

if [ "$defaults_correct" != "y" ]; then
    echo "Setup aborted."
    echo 'Set correct defaults with "az configure --defaults prop=value".'
    echo 'Set correct subscription with e.g. "az account set --subscription CheckMade"'
    exit 1
fi


# --- RUN SETUP OF AZ SERVICES FROM SCRATCH -----------------------------------------

echo "--------------------"
echo "Consider manually running selected sub-scripts if you don't set up the resource group from scratch."

az_services_setup_scripts=(
    "$SCRIPT_DIR/az_services/storage_new_setup.sh" # Updates vars: $storage_name
    "$SCRIPT_DIR/az_services/functionapp_new_setup.sh" # Updates vars: functionapp_name, functionapp_assigned_id
    "$SCRIPT_DIR/az_services/keyvault_new_setup.sh" # Updates vars: keyvault_id, keyvault_name
    "$SCRIPT_DIR/az_services/keyvault_func_map.sh" # Updates vars: keyvault_name, functionapp_name, function_assigned_id
    )

for script in "${az_services_setup_scripts[@]}"; do
    confirm_script_launch "$script"
done


# --- DEPLOYMENT PREP -----------------------------------------

confirm_script_launch "$SCRIPT_DIR/deploy_prep/telegram_tokens_to_keyvault"

echo "--------------------"
echo "INSTRUCTION: In preparation for Continuous Deployment, now go to the Azure Web Portal | \
Function: '${functionapp_name}' | Configuration (menu) | General Settings (tab) | Stack settings (section) | \
.NET Version (dropdown) and set a version and 'Save'! Continue here with 'Enter' when done."
read -r

confirm_script_launch "$SCRIPT_DIR/deploy_prep/publishing_profile_to_github.sh"

echo "--------------------"
echo "Verify deployment section of the GitHub Actions Workflow (esp. 'app-name' and 'publish_profile' properties. \
Continue with 'Enter' when done."
read -r

echo "--------------------"
echo "Verify keyvault URL in Program.cs of top-level project(s). Continue with 'Enter' when done."
read -r


# --- HOSTING-ENV-AGNOSTIC DB SETUP VARS -----------------------------------------

# These are consumed by subsequent db-setup-related scripts and are the same across all hosting environments

PG_DB_NAME="cm_ops"
export PG_DB_NAME

PG_APP_USER="cm_app_user"
export PG_APP_USER


# --- POSTGRES LOCAL/DEV CLUSTER/SERVER SETUP -----------------------------------------

PG_SUPER_USER=$(whoami)
export PG_SUPER_USER

PG_APP_USER_PSW="my_dev_db_psw" # exposing psw here for convenience is not critical, it's only the local dev db.
export PG_APP_USER_PSW

confirm_script_launch "$SCRIPT_DIR/db/dev_only/db_setup.sh" 


# --- COSMOS DB - INITIAL CLUSTER SETUP -----------------------------------------

echo "Next, go to the Azure Portal and create a 'Cosmos DB for PostgreSQL' resource if you haven't yet."
echo "--- Setting Defaults ---"

echo "BASICS: \
Cluster name: follow naming convention e.g. 'postgres-[randomCode]'; \
choose smallest scale e.g. '1 node, no high availability, Burstable, 1vCore, 2 GiB RAM'; \
generate and keep save the admin password"

# >>>> SECURITY RELEVANT: <<<<
# https://learn.microsoft.com/en-gb/azure/cosmos-db/postgresql/howto-manage-firewall-using-portal
# --> Can I not limit access to my functionapp?? No!! See:
# https://chat.openai.com/share/1a5ce5d7-0756-4695-b01d-6bf226526415  and
# https://learn.microsoft.com/en-gb/azure/azure-functions/ip-addresses?tabs=portal#consumption-and-premium-plans
echo "NETWORKING: \
Choose 'Public access (allowed IP addresses)' and \
under 'Firewall rules' activate 'Allow public access from Azure services...' \
and add the current dev machine's IP address."

echo "ENCRYPTION: \
Leave 'Service-managed key' default"

echo "Deployment of the new postgres cluster takes several minutes and can be followed under 'Deployments' section \
of the resource group. When done, enter the name of the postgres cluster to continue:"
read -r postgres_cluster_name


# --- COSMOS DB SETUP -----------------------------------------

PG_SUPER_USER="citus"
export PG_SUPER_USER

echo "Enter the password for '${PG_APP_USER}' (NOT 'citus'!!) for the prd (cosmos) db (already saved in 1psw?):"
read -r PG_APP_USER_PSW
if [[ -z "$PG_APP_USER_PSW" ]]; then
  echo "Err: No password set"
  exit 1
fi
export PG_APP_USER_PSW

confirm_script_launch "$SCRIPT_DIR/db/all_host_env/db_app_user_setup.sh" "Production"
confirm_script_launch "$SCRIPT_DIR/db/all_host_env/apply_migrations.sh"
confirm_script_launch "$SCRIPT_DIR/deploy_prep/set_db_connstring_in_funcapp.sh" 
confirm_script_launch "$SCRIPT_DIR/deploy_prep/set_db_psw_in_keyvault.sh"


# --- PUBLISH & TELEGRAM WebHooks Setup -----------------------------------------

echo "--------------------"
echo "Now publish the functions to Azure via the main GitHub Action Workflow (necessary for the next step \
to retrieve the secret code of the live function). Continue with 'Enter' when done."
read -r

echo "--------------------"
echo "If setting up Telegram WebHooks for dev AND prd then launch a second time manually!"
confirm_script_launch "$SCRIPT_DIR/clients/telegram_webhooks_config.sh"

