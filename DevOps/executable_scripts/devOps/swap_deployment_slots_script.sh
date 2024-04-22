#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------


# Not adding --subscription argument, assuming a default was set
# Not adding -g (resource group) argument, assuming a default was set 
# See '_azure_cli_admin_basic_commands.sh' for how to set defaults

echo "Setting Variables:"
functionAppName='eventops'; echo "resourceGroup=$functionAppName"
stagingSlotName='staging'; echo "stagingSlotName=$stagingSlotName"

echo "Not offering 'reset swap (r)' as an option because it just didn't work. Instead, just swap again if a reset is necessary." 
echo "Swap immediately (enter), preview-swap (p), quit (q):"
read actionArgumentInput

if [ "$actionArgumentInput" == "" ]; then
  actionArgument="swap"
elif [ "$actionArgumentInput" == "p" ]; then
  actionArgument="preview"
#elif [ "$actionArgumentInput" == "r" ]; then
#  actionArgument="reset"
elif [ "$actionArgumentInput" == "q" ]; then
  return 1
fi

echo "Swapping slots for functionapp $functionAppName..."
# --target-slot argument defaults to 'production'
az functionapp deployment slot swap -n $functionAppName --slot $stagingSlotName --action $actionArgument


# Åbout swapping back (instead of using the broken 'reset'):

# Actually, the --slot and --target-slot parameters do not need to be interchanged if you want to swap them again.
# By default, the az functionapp deployment slot swap command swaps between the specified slot (--slot) and the production slot.
# If the --target-slot argument is not provided, the production slot is assumed.
# The --target-slot argument is typically used when you want to swap between two non-production slots.

