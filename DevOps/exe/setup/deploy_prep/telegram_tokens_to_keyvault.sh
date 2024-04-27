#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

script_dir=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir/../../global_utils.sh"
source "$script_dir/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

# ToDo: Also add Staging bot tokens check
env_var_is_set "PRD_CHECKMADE_SUBMISSIONS_BOT_TOKEN" "secret"
env_var_is_set "PRD_CHECKMADE_COMMUNICATIONS_BOT_TOKEN" "secret"
env_var_is_set "PRD_CHECKMADE_NOTIFICATIONS_BOT_TOKEN" "secret"

KEYVAULT_NAME=$(confirm_and_select_resource "keyvault" "$KEYVAULT_NAME")

bot_config_section="TelegramBotConfiguration"
echo "Now setting Telegram Bot Tokens as new secrets under section '${bot_config_section}'
as per local secrets json structure..."

#  Declaring a dictionary, mapping bot_type to bot_token, based on tokens stored in the ENVIRONMENT
declare -A bots
# shellcheck disable=SC2154
# ToDo: Also add Staging bot tokens
bots["Submissions"]="$PRD_CHECKMADE_SUBMISSIONS_BOT_TOKEN"
bots["Communications"]="$PRD_CHECKMADE_COMMUNICATIONS_BOT_TOKEN"
bots["Notifications"]="$PRD_CHECKMADE_NOTIFICATIONS_BOT_TOKEN"

for bot_type in "${!bots[@]}"; do
    bot_token="${bots[$bot_type]}"
    if [ -n "$bot_token" ]; then
        # Use '--' instead of the usual ':' to access nested values e.g. 'TelegramBotConfiguration--SubmissionsBotToken'
        az keyvault secret set --vault-name "$KEYVAULT_NAME" \
        --name "${bot_config_section}--${bot_type}BotToken" --value "$bot_token"
    else
        echo "The bot_token for '${bot_type}' is empty, therefore, not setting it as a secret in keyvault."
    fi
done

echo "All BotTokens have been added as secrets to the keyvault '${KEYVAULT_NAME}'. Currently saved secrets are:"
az keyvault secret list --vault-name "$KEYVAULT_NAME" --query "[*].name" --output tsv
