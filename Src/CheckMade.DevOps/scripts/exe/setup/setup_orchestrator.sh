#!/usr/bin/env bash

# Exit immediately if a command exits with a non-zero status (including in the middle of a pipeline).
set -e 
set -o pipefail

script_dir_orchestrator=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir_orchestrator/../script_utils.sh"

# -------------------------------------------------------------------------------------------------------

# #######################################################################################################
# This meta-script serves as executable documentation for the setup of Azure / Bot / DB from scratch. 
# It also represents an overview / menu of config options outside of 'full setup from scratch'.
# #######################################################################################################

# --- PREPARATION ------------------------------------------------

echo "--------------------"
echo "IMPORTANT: You must launch this script from the CheckMade repo's root directory for 'gh' to refer to the \
correct repo!"
working_dir_is_solution_root
echo "--------------------"
echo "Make sure these preparatory steps have been completed:"
echo "- Azure Portal account and 'CheckMade' subscription have been set up"
echo "- Logged in to Azure CLI and default account and location have been set (use 'az_init/az_cli_init.sh')"
echo "- All needed bots have been created with the Telegram BotFather and their credentials/tokens saved in the \
ENVIRONMENT for access by subsequent scripts; the dev-related tokens have been saved in secrets.json of the \
Functions StartUp project."
echo "- 'GITHUB_TOKEN' in ENVIRONMENT has been set to a GitHub PAT that gives gh comprehensive admin access"

echo "--------------------"
confirm_command \
"Need to go back to do some prep steps (y/n)?" \
"exit 0"

# --- CONFIRM AZ SETTINGS / CREATE RESOURCE GROUP --------------------------------

echo "--------------------"
echo "Current Subscription:"
az account show --query name

echo "Current Defaults:"
az configure --list-defaults 

echo "Currently available resource groups:"
az group list --query "[*].name"

echo "--------------------"
echo "Do you want to create a new resource group for az services?"
confirm_script_launch "$script_dir_orchestrator/az_init/group_new_setup.sh"

echo "--------------------"
CURRENT_COMMON_RESOURCE_GROUP="common-d425V"
echo "The current 'common' resource group (e.g. used for the production DB) is saved as: \
'${CURRENT_COMMON_RESOURCE_GROUP}'. It is 'common' in the same sense as the CheckMade.Common.XYZ code modules."
echo ""

echo "--------------------"
echo "Confirm if current subscription, defaults and common resource group are correct (y/n)"
read -r defaults_correct

if [ "$defaults_correct" != "y" ]; then
    echo "Setup aborted."
    echo 'Set correct defaults with "az configure --defaults prop=value".'
    echo 'Set correct subscription with e.g. "az account set --subscription CheckMade"'
    exit 0
fi

# --- RUN SETUP OF AZ SERVICES FROM SCRATCH -----------------------------------------

echo "--------------------"
echo "Consider manually running selected sub-scripts if you don't set up the resource group from scratch."

az_services_setup_scripts=(
    "$script_dir_orchestrator/az_services/storage_new_setup.sh" # Updates vars: $STORAGE_NAME
    "$script_dir_orchestrator/az_services/functionapp_new_setup.sh" # Updates vars: FUNCTIONAPP_NAME
    "$script_dir_orchestrator/az_services/keyvault_new_setup.sh" # Updates vars: KEYVAULT_NAME
    "$script_dir_orchestrator/az_services/keyvault_func_map.sh" # Updates vars: KEYVAULT_NAME, FUNCTIONAPP_NAME
    )

for script in "${az_services_setup_scripts[@]}"; do
    confirm_script_launch "$script"
done

# --- DEPLOYMENT PREP -----------------------------------------

confirm_script_launch "$script_dir_orchestrator/deploy_prep/telegram_tokens_to_keyvault.sh"

echo "--------------------"
confirm_script_launch "$script_dir_orchestrator/deploy_prep/publishing_profile_to_github.sh"

echo "--------------------"
echo "Verify deployment section of the GitHub Actions Workflow (esp. 'app-name' and 'publish_profile' properties. \
Continue with 'Enter' when done."
read -r

confirm_script_launch "$script_dir_orchestrator/deploy_prep/set_blobstorage_settings_in_keyvault_and_env.sh"

echo "--------------------"
echo "Verify keyvault URL in Program.cs of top-level project(s)."
if [ -n "$KEYVAULT_NAME" ]; then
  echo "Current keyvault name is: '${KEYVAULT_NAME}', usually enough to verify the alphanumeric code in the URL."
fi
echo "Continue with 'Enter' when done."
read -r

# --- HOSTING-ENV-AGNOSTIC DB SETUP VARS -----------------------------------------

# These are consumed by subsequent db-setup-related scripts and are the same across all hosting environments
# They should be configured in the environment (~/.zprofile) as they are also consumed by other, non-setup scripts.
env_var_is_set "PG_DB_NAME"
env_var_is_set "PG_APP_USER"
env_var_is_set "PG_SUPER_USER_DEV"
env_var_is_set "PG_SUPER_USER_PRD"

# --- POSTGRES LOCAL/DEV CLUSTER/SERVER SETUP -----------------------------------------

echo "Setting vars for (dev) DB Setup..."
PG_SUPER_USER=$PG_SUPER_USER_DEV
PG_APP_USER_PSW="my_dev_db_psw" # not security critical, save also in in local.settings.json connstring
echo "PG_APP_USER_PSW was set to '${PG_APP_USER_PSW}'. Make sure it's the same in local.settings.json files \
of all toplevel projects (including Test projects!!)."

confirm_script_launch "$script_dir_orchestrator/db/dev_only/db_setup.sh" 

# --- COSMOS DB - INITIAL CLUSTER SETUP -----------------------------------------

echo "-------------"
echo "Next, go to the Azure Portal and create a 'Cosmos DB for PostgreSQL' resource in the \
'${CURRENT_COMMON_RESOURCE_GROUP}' resource group, if you haven't yet."
echo "--- Setting Defaults ---"

echo "BASICS:"
echo "Cluster name: follow naming convention e.g. 'postgres-[randomCode]'; \
choose smallest scale e.g. '1 node, no high availability, Burstable, 1vCore, 2 GiB RAM'; \
Database name '${PG_DB_NAME}'; generate and keep save the admin password"

# >>>> SECURITY RELEVANT: <<<<
# https://learn.microsoft.com/en-gb/azure/cosmos-db/postgresql/howto-manage-firewall-using-portal
# --> Can I not limit access to my functionapp?? No!! See:
# https://chat.openai.com/share/1a5ce5d7-0756-4695-b01d-6bf226526415  and
# https://learn.microsoft.com/en-gb/azure/azure-functions/ip-addresses?tabs=portal#consumption-and-premium-plans
echo "NETWORKING:"
echo "Choose 'Public access (allowed IP addresses)' and \
under 'Firewall rules' activate 'Allow public access from Azure services...' \
and add the current dev machine's IP address."

echo "ENCRYPTION:"
echo "Leave 'Service-managed key' default"
echo "-----"
echo "Deployment of the new postgres cluster takes several minutes and can be followed under 'Deployments' section \
of the resource group. When done, or when already exists, enter the name of the postgres cluster to continue:"
read -r COSMOSDB_PG_CLUSTER_NAME

# --- COSMOS DB SETUP -----------------------------------------

PG_SUPER_USER=$PG_SUPER_USER_PRD

# Forces psql, when connecting from the dev's machine into an Azure CosmosDb, to use the specified settings.
PGSSLMODE=verify-full
PGSSLROOTCERT=~/AzureCosmosDbForPostgreSQLDigiCert.crt.pem

echo "------------"
echo "Set password for App User '${PG_APP_USER}' (y/n)? Only needed if you still want to set up 'db_app_user' or \
set the app user's psw in keyvault."
read -r answer_set_psw

if [ "$answer_set_psw" == "y" ]; then
  echo "Enter/set the password for '${PG_APP_USER}' (NOT 'citus'!!) for the (prd) CosmosDb (save in psw-manager!):"
  read -r PG_APP_USER_PSW
  if [ -z "$PG_APP_USER_PSW" ]; then
    echo "Err: No password set"
    exit 1
  fi  
fi

# Needed in multiple of the following sub scripts
COSMOSDB_PG_HOST="$(az cosmosdb postgres cluster show -g $CURRENT_COMMON_RESOURCE_GROUP -n "$COSMOSDB_PG_CLUSTER_NAME" \
--query "serverNames[*].fullyQualifiedDomainName" --output tsv)"

echo "COSMOSDB_PG_HOST was set to '${COSMOSDB_PG_HOST}'."
echo "Make sure this current value is also saved into the global \
environment (e.g. ~/.zprofile), as it's needed by other, non-setup scripts!"

confirm_script_launch "$script_dir_orchestrator/db/all_host_env/db_app_user_setup.sh" "Production"
confirm_script_launch "$script_dir_orchestrator/db/all_host_env/apply_migrations.sh" "Production"
confirm_script_launch "$script_dir_orchestrator/deploy_prep/set_db_connstring_in_funcapp.sh" 
confirm_script_launch "$script_dir_orchestrator/deploy_prep/set_db_psw_in_keyvault.sh"

# --- PUBLISH & TELEGRAM WebHooks Setup -----------------------------------------

echo "--------------------"
echo "Now publish the functions to Azure (e.g. via the main GitHub Action Workflow) to allow retrieval of secret codes \
of deployed functions - which in turn prepares us for the next step of setting Telegram Webhooks. \
Continue with 'Enter' when done publishing."
read -r
echo "---------------------"
echo "In case this was a setup from scratch, first set the WebHooks only for the staging slot!"
echo "Then do the first 'swap' of the production vs. staging slots, and only then set the WebHooks for production."
echo "Continue with 'Enter'."
read -r
confirm_script_launch "$script_dir_orchestrator/deploy_prep/telegram_webhooks_config.sh"
echo "---------------------"
echo "Congratulations, you reached the end of the setup orchestration script!"
