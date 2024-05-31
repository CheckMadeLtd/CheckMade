#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../script_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "Checking necessary environment variables are set..."

env_var_is_set "DEV_CHECKMADE_OPERATIONS_BOT_TOKEN" "secret"
env_var_is_set "DEV_CHECKMADE_COMMUNICATIONS_BOT_TOKEN" "secret"
env_var_is_set "DEV_CHECKMADE_NOTIFICATIONS_BOT_TOKEN" "secret"

env_var_is_set "STG_CHECKMADE_OPERATIONS_BOT_TOKEN" "secret"
env_var_is_set "STG_CHECKMADE_COMMUNICATIONS_BOT_TOKEN" "secret"
env_var_is_set "STG_CHECKMADE_NOTIFICATIONS_BOT_TOKEN" "secret"

env_var_is_set "PRD_CHECKMADE_OPERATIONS_BOT_TOKEN" "secret"
env_var_is_set "PRD_CHECKMADE_COMMUNICATIONS_BOT_TOKEN" "secret"
env_var_is_set "PRD_CHECKMADE_NOTIFICATIONS_BOT_TOKEN" "secret"

echo "  "
echo "--- Welcome to the Telegram Bot Setup Tool ---"
echo "Here you can manage the WebHook of an EXISTING Telegram Bot (created with BotFather) as found in ENVIRONMENT"

while true; do
  echo "---"
  echo "Please choose the bot by entering the two-digit id (or 'q' to exit):"
  
  echo "ds = (dev) Operations Bot"
  echo "dc = (dev) Communications Bot"
  echo "dn = (dev) Notifications Bot"
  echo " "
  echo "ss = (stg) Operations Bot"
  echo "sc = (stg) Communications Bot"
  echo "sn = (stg) Notifications Bot"
  echo " "
  echo "ps = (prd) Operations Bot"
  echo "pc = (prd) Communications Bot"
  echo "pn = (prd) Notifications Bot"
  
  read -r bot_choice
  
  if [ -z "$bot_choice" ]; then
    echo "No bot was chosen, aborting"
    return 0
  
  elif [ "$bot_choice" == "ds" ]; then
    bot_token="$DEV_CHECKMADE_OPERATIONS_BOT_TOKEN"
  elif [ "$bot_choice" == "dc" ]; then
    bot_token="$DEV_CHECKMADE_COMMUNICATIONS_BOT_TOKEN"
  elif [ "$bot_choice" == "dn" ]; then
    bot_token="$DEV_CHECKMADE_NOTIFICATIONS_BOT_TOKEN"
  
  elif [ "$bot_choice" == "ss" ]; then
    bot_token="$STG_CHECKMADE_OPERATIONS_BOT_TOKEN"
  elif [ "$bot_choice" == "sc" ]; then
    bot_token="$STG_CHECKMADE_COMMUNICATIONS_BOT_TOKEN"
  elif [ "$bot_choice" == "sn" ]; then
    bot_token="$STG_CHECKMADE_NOTIFICATIONS_BOT_TOKEN"
  
  elif [ "$bot_choice" == "ps" ]; then
    bot_token="$PRD_CHECKMADE_OPERATIONS_BOT_TOKEN"
  elif [ "$bot_choice" == "pc" ]; then
    bot_token="$PRD_CHECKMADE_COMMUNICATIONS_BOT_TOKEN"
  elif [ "$bot_choice" == "pn" ]; then
    bot_token="$PRD_CHECKMADE_NOTIFICATIONS_BOT_TOKEN"
  
  elif [ "$bot_choice" == "q" ]; then
    return 0
  else
    echo "Err: No valid bot choice, aborting"
    exit 1
  fi
  
  echo "What would you like to do? Set WebHook (default behaviour, continue with 'Enter') or \
  get current WebhookInfo (enter 'g')?"
  read -r bot_setup_task
  
  if [ "$bot_setup_task" == "g" ]; then
    curl --request POST --url https://api.telegram.org/bot"$bot_token"/getWebhookInfo
    return 0
  fi
  
  bot_type=${bot_choice:1:1} # the second letter
  
  if [ "$bot_type" == "s" ]; then
    function_name="OperationsBot"
  elif [ "$bot_type" == "c" ]; then
    function_name="CommunicationsBot"
  elif [ "$bot_type" == "n" ]; then
    function_name="NotificationsBot"
  fi
  
  bot_hosting_context=${bot_choice:0:1} # the first letter
  
  if [ "$bot_hosting_context" == "d" ]; then # dev
  
    echo "Please enter the https function endpoint host (use 'ngrok http 7071' in a separate CLI instance to generate \
  the URL that forwards to localhost)"
    read -r functionapp_endpoint
    functionapp_endpoint="$functionapp_endpoint/api/${function_name,,}" # ,, = to lower case
    
  else # not dev
  
    echo "Select functionapp to connect to Telegram..."
    FUNCTIONAPP_NAME=$(confirm_and_select_resource "functionapp" "$FUNCTIONAPP_NAME")
    echo "Now retrieving function code (wait!) and determining endpoint ..."

    if [ "$bot_hosting_context" == "s" ]; then # staging
      
      functionapp_with_slot="$FUNCTIONAPP_NAME-staging"

      function_code=$(az functionapp function keys list \
      -n "$FUNCTIONAPP_NAME" --function-name "$function_name" \
      --slot 'staging' --query default --output tsv)
        
    elif [ "$bot_hosting_context" == "p" ]; then # production
      
      functionapp_with_slot="$FUNCTIONAPP_NAME"

      function_code=$(az functionapp function keys list \
      -n "$FUNCTIONAPP_NAME" --function-name "$function_name" \
      --query default --output tsv)

    fi
    
    # ,, = to lower case
    functionapp_endpoint="https://$functionapp_with_slot.azurewebsites.net/api/${function_name,,}?code=$function_code"
        
  fi
  
  echo "FYI your function endpoint with gateway is:"
  echo "$functionapp_endpoint"
  echo "Now setting Webhook..."
  
  curl --request POST \
  --url https://api.telegram.org/bot"$bot_token"/setWebhook \
  --header 'content-type: application/json' \
  --data '{"url": "'"$functionapp_endpoint"'"}'

done
