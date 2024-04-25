#!/opt/homebrew/bin/bash

# Exit immediately if a command exits with a non-zero status (including in the middle of a pipeline).
set -e 
set -o pipefail

SCRIPT_DIR=$(dirname "${BASH_SOURCE[0]}")
source "$SCRIPT_DIR"/setup_utilities.sh

# -------------------------------------------------------------------------------------------------------

echo "--- Welcome to the Telegram Bot Setup Tool ---"
echo "Here you can manage the WebHook of an EXISTING Telegram Bot (created with BotFather) as found in ENVIRONMENT"

echo "Please choose the bot by entering the two-digit id:"
echo "ds = (dev) Submissions Bot"
echo "dc = (dev) Communications Bot"
echo "dn = (dev) Notifications Bot"
echo "ps = (prd) Submissions Bot"
echo "pc = (prd) Communications Bot"
echo "pn = (prd) Notifications Bot"

read -r bot_choice

if [ -z "$bot_choice" ]; then
  echo "No bot was chosen, aborting"
  exit 0
elif [ "$bot_choice" == "ds" ]; then
  bot_token="$devCheckMadeSubmissionsBotToken"
elif [ "$bot_choice" == "dc" ]; then
  bot_token="$devCheckMadeCommunicationsBotToken"
elif [ "$bot_choice" == "dn" ]; then
  bot_token="$devCheckMadeNotificationsBotToken"
elif [ "$bot_choice" == "ps" ]; then
  bot_token="$CheckMadeSubmissionsBotToken"
elif [ "$bot_choice" == "pc" ]; then
  bot_token="$CheckMadeCommunicationsBotToken"
elif [ "$bot_choice" == "pn" ]; then
  bot_token="$CheckMadeNotificationsBotToken"
else
  echo "No valid bot choice, aborting"
  exit 1
fi

echo "What would you like to do? Set WebHook (default behaviour, continue with 'Enter') or \
get current WebhookInfo (enter 'g')?"
read -r bot_setup_task

if [ "$bot_setup_task" == "g" ]; then
  curl --request POST --url https://api.telegram.org/bot"$bot_token"/getWebhookInfo
  exit 0
fi

bot_hosting_context=${bot_choice:0:1} # the first letter

if [ "$bot_hosting_context" == "d" ]; then

  echo "Please enter the https function endpoint host (use 'ngrok http 7071' in a separate CLI instance to generate \
the URL that forwards to localhost)"
  read -r functionapp_endpoint

elif [ "$bot_hosting_context" == "p" ]; then 

  echo "Select functionapp to connect to Telegram..."
  functionapp_name=$(confirm_and_select_resource "functionapp" "$functionapp_name")
  functionapp_endpoint="https://$functionapp_name.azurewebsites.net"
  echo "The function endpoint has been set to: $functionapp_endpoint"
  
  function_code=$(az functionapp function keys list \
  -n "$functionapp_name" --function-name "$function_name" \
  --query default --output tsv)
  
  functionapp_endpoint="$functionapp_endpoint?code=$function_code"
fi

bot_type=${bot_choice:1:1} # the second letter

if [ "$bot_type" == "s" ]; then
  function_name=SubmissionsBot
  functionapp_endpoint="$functionapp_endpoint/api/${function_name,,}" # ,, = to lower case
elif [ "$bot_type" == "c" ]; then
  function_name=CommunicationsBot
  functionapp_endpoint="$functionapp_endpoint/api/${function_name,,}"
elif [ "$bot_type" == "n" ]; then
  function_name=NotificationsBot
  functionapp_endpoint="$functionapp_endpoint/api/${function_name,,}"
fi

echo "FYI your function endpoint with gateway is:"
echo "$functionapp_endpoint"
echo "Now setting Webhook..."

curl --request POST \
--url https://api.telegram.org/bot"$bot_token"/setWebhook \
--header 'content-type: application/json' \
--data '{"url": "'"$functionapp_endpoint"'"}'

