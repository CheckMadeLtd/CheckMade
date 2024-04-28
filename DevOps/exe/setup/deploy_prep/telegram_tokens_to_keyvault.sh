#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

# ToDo: Also add Staging bot tokens check
echo "Checking necessary environment variables are set..."
env_var_is_set "PRD-CHECKMADE-SUBMISSIONS-BOT-TOKEN" "secret"
env_var_is_set "PRD-CHECKMADE-COMMUNICATIONS-BOT-TOKEN" "secret"
env_var_is_set "PRD-CHECKMADE-NOTIFICATIONS-BOT-TOKEN" "secret"

KEYVAULT_NAME=$(confirm_and_select_resource "keyvault" "$KEYVAULT_NAME")

bot_config_section="TelegramBotConfiguration"
echo "Now setting Telegram Bot Tokens as new secrets under section '${bot_config_section}'
as per local secrets json structure..."

#  Declaring a dictionary, mapping bot_type to bot_token, based on tokens stored in the ENVIRONMENT
declare -A bots
# shellcheck disable=SC2154
# ToDo: Also add Staging bot tokens
bots["CHECKMADE-SUBMISSIONS"]="$PRD-CHECKMADE-SUBMISSIONS-BOT-TOKEN"
bots["CHECKMADE-COMMUNICATIONS"]="$PRD-CHECKMADE-COMMUNICATIONS-BOT-TOKEN"
bots["CHECKMADE-NOTIFICATIONS"]="$PRD-CHECKMADE-NOTIFICATIONS-BOT-TOKEN"

for bot_type in "${!bots[@]}"; do
    bot_token="${bots[$bot_type]}"
    if [ -n "$bot_token" ]; then
          # Use '--' instead of the usual ':' to access nested values in Keyvault
          # This leads to --name = e.g. 'TelegramBotConfiguration--CHECKMADE-SUBMISSIONS-BOT-TOKEN'
          az keyvault secret set --vault-name "$KEYVAULT_NAME" \
        --name "${bot_config_section}--${bot_type}-BOT-TOKEN" --value "$bot_token"
    else
        echo "The bot_token for '${bot_type}' is empty, therefore, not setting it as a secret in keyvault."
    fi
done

echo "All BotTokens have been added as secrets to the keyvault '${KEYVAULT_NAME}'. Currently saved secrets are:"
az keyvault secret list --vault-name "$KEYVAULT_NAME" --query "[*].name" --output tsv
