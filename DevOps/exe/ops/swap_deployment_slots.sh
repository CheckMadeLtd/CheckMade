#!/usr/bin/env bash
set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------


# Not adding --subscription argument, assuming a default was set
# Not adding -g (resource group) argument, assuming a default was set 
# See '_azure_cli_admin_basic_commands.sh' for how to set defaults

echo "Setting Variables:"
FUNCTIONAPP_NAME='eventops'; echo "function_app_name=$FUNCTIONAPP_NAME"
staging_slot_name='staging'; echo "staging_slot_name=$staging_slot_name"

echo "Not offering 'reset swap (r)' as an option because it just didn't work. Instead, just swap again if a reset is \
necessary.Swap immediately (enter), preview-swap (p), quit (q):"
read -r action_argument_input

if [ "$action_argument_input" == "" ]; then
  action_argument="swap"
elif [ "$action_argument_input" == "p" ]; then
  action_argument="preview"
#elif [ "$action_argument_input" == "r" ]; then
#  action_argument="reset"
elif [ "$action_argument_input" == "q" ]; then
  exit 0
fi

echo "Swapping slots for functionapp $FUNCTIONAPP_NAME..."
# --target-slot argument defaults to 'production'
az functionapp deployment slot swap -n $FUNCTIONAPP_NAME --slot $staging_slot_name --action "$action_argument"


# Åbout swapping back (instead of using the broken 'reset'):

# Actually, the --slot and --target-slot parameters do not need to be interchanged if you want to swap them again.
# By default, the az functionapp deployment slot swap command swaps between the specified slot (--slot) and the production slot.
# If the --target-slot argument is not provided, the production slot is assumed.
# The --target-slot argument is typically used when you want to swap between two non-production slots.

