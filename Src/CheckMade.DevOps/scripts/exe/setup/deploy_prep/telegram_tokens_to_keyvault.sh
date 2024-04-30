#!/usr/bin/env bash
set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Checking necessary environment variables are set..."

env_var_is_set "STG_CHECKMADE_SUBMISSIONS_BOT_TOKEN" "secret"
env_var_is_set "STG_CHECKMADE_COMMUNICATIONS_BOT_TOKEN" "secret"
env_var_is_set "STG_CHECKMADE_NOTIFICATIONS_BOT_TOKEN" "secret"

env_var_is_set "PRD_CHECKMADE_SUBMISSIONS_BOT_TOKEN" "secret"
env_var_is_set "PRD_CHECKMADE_COMMUNICATIONS_BOT_TOKEN" "secret"
env_var_is_set "PRD_CHECKMADE_NOTIFICATIONS_BOT_TOKEN" "secret"

KEYVAULT_NAME=$(confirm_and_select_resource "keyvault" "$KEYVAULT_NAME")

bot_config_section="TelegramBotConfiguration"
echo "Now setting Telegram Bot Tokens as new secrets under section '${bot_config_section}'
as per local secrets json structure..."

#  Declaring a dictionary, mapping bot_type to bot_token, based on tokens stored in the ENVIRONMENT
#  In the Unix Env. the keys need to use '_' but in dotnet / Azure Keyvault they need to use '-'!!
declare -A bots

bots["STG-CHECKMADE-SUBMISSIONS"]="$STG_CHECKMADE_SUBMISSIONS_BOT_TOKEN"
bots["STG-CHECKMADE-COMMUNICATIONS"]="$STG_CHECKMADE_COMMUNICATIONS_BOT_TOKEN"
bots["STG-CHECKMADE-NOTIFICATIONS"]="$STG_CHECKMADE_NOTIFICATIONS_BOT_TOKEN"

bots["PRD-CHECKMADE-SUBMISSIONS"]="$PRD_CHECKMADE_SUBMISSIONS_BOT_TOKEN"
bots["PRD-CHECKMADE-COMMUNICATIONS"]="$PRD_CHECKMADE_COMMUNICATIONS_BOT_TOKEN"
bots["PRD-CHECKMADE-NOTIFICATIONS"]="$PRD_CHECKMADE_NOTIFICATIONS_BOT_TOKEN"

for bot_type in "${!bots[@]}"; do
    bot_token="${bots[$bot_type]}"
    if [ -n "$bot_token" ]; then
          # Use '--' instead of the usual ':' to access nested values in Keyvault
          # This leads to --name = e.g. 'TelegramBotConfiguration--CHECKMADE_SUBMISSIONS_BOT_TOKEN'
          az keyvault secret set --vault-name "$KEYVAULT_NAME" \
        --name "${bot_config_section}--${bot_type}-BOT-TOKEN" --value "$bot_token"
    else
        echo "The bot_token for '${bot_type}' is empty, therefore, not setting it as a secret in keyvault."
    fi
done

echo "All BotTokens have been added as secrets to the keyvault '${KEYVAULT_NAME}'. Currently saved secrets are:"
az keyvault secret list --vault-name "$KEYVAULT_NAME" --query "[*].name" --output tsv
