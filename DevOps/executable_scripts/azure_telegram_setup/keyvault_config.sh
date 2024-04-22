#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "$BASH_SOURCE")
source $SCRIPT_DIR/setup_utilities.sh

# -------------------------------------------------------------------------------------------------------

keyvault_name=$(confirm_and_select_resource "keyvault" "$keyvault_name")

keyvault_id=$(az keyvault show --name $keyvault_name --query id --output tsv)
echo "The id of the chosen keyvault is: $keyvault_id"

echo "To configure a keyvault, a functionapp needs to gain read access to it."
functionapp_name=$(confirm_and_select_resource "functionapp" "$functionapp_name")
functionapp_assigned_id=$(az functionapp identity show --name $functionapp_name --query principalId --output tsv)

role_readonly="Key Vault Secrets User"
echo "Now assigning keyvault read access rights (new or existing keyvault) to the selected function..."
az role assignment create --assignee $functionapp_assigned_id --scope $keyvault_id --role "$role_readonly"

bot_config_section="TelegramBotConfiguration"
echo "Now setting Telegram Bot Tokens as new secrets under section '${bot_config_section}'"\
    "as per local secrets json structure..."

#  Declaring a dictionary, mapping bot_type to bot_token, based on tokens stored in the ENVIRONMENT
declare -A bots
bots[Submissions]=$CheckMadeSubmissionsBot
bots[Communications]=$CheckMadeCommunicationsBot
bots[Notifications]=$CheckMadeNotificationsBot

for bot_type in "${!bots[@]}"; do
    bot_token="${bots[$bot_type]}"
    if [ -n "$bot_token" ]; then
        # Use '--' instead of the usual ':' to access nested values e.g. 'TelegramBotConfiguration--SubmissionsBotToken'
        az keyvault secret set --vault-name "$keyvault_name" \
        --name "${bot_config_section}--${bot_type}BotToken" --value "$bot_token"
    else
        echo "The bot_token for '${bot_type}' is empty, therefore, not setting it as a secret in keyvault."
    fi
done

echo "All BotTokens have been added as secrets to the keyvault '${keyvault_name}'. Currently saved secrets are:"
az keyvault secret list --vault-name $keyvault_name --query "[*].name" --output tsv
