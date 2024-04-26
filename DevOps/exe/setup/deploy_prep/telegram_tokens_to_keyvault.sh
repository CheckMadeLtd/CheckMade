#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

keyvault_name=$(confirm_and_select_resource "keyvault" "$keyvault_name")

bot_config_section="TelegramBotConfiguration"
echo "Now setting Telegram Bot Tokens as new secrets under section '${bot_config_section}'
as per local secrets json structure..."

#  Declaring a dictionary, mapping bot_type to bot_token, based on tokens stored in the ENVIRONMENT
declare -A bots
# shellcheck disable=SC2154
bots["Submissions"]=$CheckMadeSubmissionsBotToken
bots["Communications"]=$CheckMadeCommunicationsBotToken
bots["Notifications"]=$CheckMadeNotificationsBotToken

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
az keyvault secret list --vault-name "$keyvault_name" --query "[*].name" --output tsv
