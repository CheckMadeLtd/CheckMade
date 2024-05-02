#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../global_utils.sh"
source "$(dirname "${BASH_SOURCE[0]}")/../setup/az_setup_utils.sh"

# -------------------------------------------------------------------------------------------------------

# inspiration from https://demiliani.com/2020/12/02/moving-azure-functions-from-consumption-to-premium-plans/

echo "FYI, Current Defaults:"
az configure --list-defaults 

# One consumption plan should always exist, because it doesn't need to be deleted, even while not using it
consumption_plan_sku="Y1"
consumption_plan_name=$(az functionapp plan list --query "[?sku.name=='$consumption_plan_sku'].name | [0]" --output tsv)

# Premium plan should only exist while using it, it's deleted when toggling away from it
premium_plan_sku='EP1'
premium_plans_in_existence=$(az functionapp plan list --query "[?sku.name=='$premium_plan_sku'] | length(@)")

functionapp_name=$(confirm_and_select_resource "functionapp")
current_active_plan=$(az functionapp show -n "$functionapp_name" --query appServicePlanId --output tsv)
current_active_sku=$(az functionapp plan show --ids $current_active_plan --query "[sku.name]" --output tsv)

if [ "$current_active_sku" == "$consumption_plan_sku" ]; then
  
  echo "FYI: $functionapp_name is currently running on the (non-premium) consumption plan '$current_active_plan')."
  echo "Confirm toggle to a premium plan with sku: '$premium_plan_sku' (y/n):"
  read -r confirm_toggle
  
  if [ "$confirm_toggle" == "y" ]; then
    
    if [ "$premium_plans_in_existence" == 0 ]; then
      premium_plan_name="chat_premium_plan_$(get_random_id)"
      echo "Creating $premium_plan_name..."
      # You can also specify the numbers of minimum and maximum instances to have and other parameters to customize your workload needs.
      az functionapp plan create --name $premium_plan_name --sku $premium_plan_sku
    else
      premium_plan_name=$(az functionapp plan list --query "[?sku.name=='$premium_plan_sku'].name | [0]" --output tsv)
      echo "Using existent premium plan '$premium_plan_name'..."
      echo "Also, check what went wrong, a premium plan usually gets deleted when toggling away from it. \
      Why does this one exist then?!"
    fi
          
    # Swapping slots seems to also swap the plan! ==> Need to toggle both slots! 
    echo "Moving function $functionapp_name to $premium_plan_name for both deployment slots..."
    az functionapp update --name "$functionapp_name" --plan "$premium_plan_name"
    az functionapp update --name "$functionapp_name" --slot 'staging' --plan "$premium_plan_name"
    
  fi

elif [ "$current_active_sku" == "$premium_plan_sku" ]; then
  
  echo "FYI: $functionapp_name is currently running on the premium plan '$current_active_plan'"
  echo "Confirm toggle back to the non-premium consumption plan '$consumption_plan_name' and deletion of current \
  premium plan '$current_active_plan' (y/n):"
  read -r confirm_toggle
  
  if [ "$confirm_toggle" == "y" ]; then
    
    echo "Downgrading '$functionapp_name' (both deployment slots) back to consumption plan '$consumption_plan_name'..."
    az functionapp update --name "$functionapp_name" --plan "$consumption_plan_name" --force
    az functionapp update --name "$functionapp_name" --slot 'staging' --plan "$consumption_plan_name" --force
    
    echo "Now deleting premium plan '$current_active_plan'..."
    az functionapp plan delete --ids "$current_active_plan"
    
  fi
  
else

  echo "An unexpected value for current_active_sku of '$current_active_sku'"
  exit 1

fi


# Background info:

# Azure Functions can run on the Azure App Service platform. In the App Service platform, plans that host Premium plan function apps
# are referred to as Elastic Premium plans, with SKU names like EP1. If you choose to run your function app on a Premium plan,
# make sure to create a plan with an SKU name that starts with "E", such as EP1. App Service plan SKU names that start with "P",
# such as P1V2 (Premium V2 Small plan), are actually Dedicated hosting plans. Because they are Dedicated and not Elastic Premium,
# plans with SKU names starting with "P" won't scale dynamically and may increase your costs.
# from https://learn.microsoft.com/en-us/azure/azure-functions/functions-premium-plan?tabs=portal
