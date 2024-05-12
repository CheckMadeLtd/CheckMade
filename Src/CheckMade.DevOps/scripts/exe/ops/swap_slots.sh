#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../script_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../setup/az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

echo "FYI, Current Defaults:"
az configure --list-defaults 

functionapp_name=$(confirm_and_select_resource "functionapp")

echo "Swap immediately (enter), preview-swap (p), reverse preview with reset (r) or quit (q):"
read -r action_argument_input

if [ "$action_argument_input" == "" ]; then
  action_argument="swap"
elif [ "$action_argument_input" == "p" ]; then
  action_argument="preview"
elif [ "$action_argument_input" == "r" ]; then
  action_argument="reset"
elif [ "$action_argument_input" == "q" ]; then
  exit 0
fi

echo "Applying swap action '${action_argument}' for functionapp $functionapp_name..."
# --target-slot argument defaults to 'production' i.e. never needed since we don't have more than one non-prd slot.
az functionapp deployment slot swap -n $functionapp_name --slot 'staging' --action "$action_argument"



# --- Reset vs. normal Swap Back: ----

# Use 'reset' only to reset a preview slot
# Use normal 'swap' to swap back a fully completed swap


# --- About using 'preview' (explanation from Azure Portal) ---

## Swap with preview breaks down a normal swap into two phases. In phase one, any slot-specific application settings 
# and connections strings on the destination will be temporarily copied to the source slot. This allows you to test 
# the slot with its final configuration values. From here, you may choose to either cancel phase one to revert to your
# normal configuration, or proceed to phase two, which would remove the temporary config changes and complete swapping
# the source to destination slot.

# ==> Preview not relevant to us especially as long as Staging and Production both access the same Keyvault and DB
# (but if we use preview, to complete the swap (phase-2), just run the normal swap operation!  