#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------


# from https://demiliani.com/2020/12/02/moving-azure-functions-from-consumption-to-premium-plans/

echo "Setting Variables:"
resourceGroup='eventops'; echo "resourceGroup=$resourceGroup"
functionAppName='eventops'; echo "functionAppName=$functionAppName"
stagingSlotName='staging'; echo "stagingSlotName=$stagingSlotName"
consumptionPlanName='ASP-eventops-bb32'; echo "consumptionPlanName=$consumptionPlanName"
premiumPlanName='eventops_premium_plan'; echo "premiumPlanName=$premiumPlanName"
skuPlan='EP1'; echo "skuPlan=$skuPlan"

echo "Would you like to upgrade to $premiumPlanName (u) , downgrade to $consumptionPlanName (d) or quit (q)?"
read activity

if [ "$activity" == "q" ]; then
  exit 0

elif [ "$activity" == 'u' ]; then
  echo "Creating $premiumPlanName..."
  # You can also specify the numbers of minimum and maximum instances to have and other parameters to customize your workload needs.
  az functionapp plan create --name $premiumPlanName --resource-group $resourceGroup --location uksouth --sku $skuPlan
  echo "Moving function $functionAppName to $premiumPlanName..."
  az functionapp update --name $functionAppName --resource-group $resourceGroup --plan $premiumPlanName
  # Not asking for whether upgrading staging or not, because swapping seems to also swap the plan!!! So always swap both. 
  echo "Moving $stagingSlotName slot of function $functionAppName to $premiumPlanName..."
  az functionapp update --name $functionAppName --resource-group $resourceGroup --slot $stagingSlotName --plan $premiumPlanName

elif [ "$activity" == "d" ]; then
  echo "Scaling back down Function app to Consumption Plan $consumptionPlanName..."
  az functionapp update --name $functionAppName --resource-group $resourceGroup --plan $consumptionPlanName --force
  echo "Scaling back down $stagingSlotName slot of function app to Consumption Plan $consumptionPlanName..."
  az functionapp update --name $functionAppName --resource-group $resourceGroup --slot $stagingSlotName --plan $consumptionPlanName --force
  
  # Azure has a "are you sure (y/n) message, so I don't need one. 
  echo "Now deleting Premium Plan $premiumPlanName..."
  az functionapp plan delete --resource-group $resourceGroup --name $premiumPlanName

fi


# Background info:

# Azure Functions can run on the Azure App Service platform. In the App Service platform, plans that host Premium plan function apps
# are referred to as Elastic Premium plans, with SKU names like EP1. If you choose to run your function app on a Premium plan,
# make sure to create a plan with an SKU name that starts with "E", such as EP1. App Service plan SKU names that start with "P",
# such as P1V2 (Premium V2 Small plan), are actually Dedicated hosting plans. Because they are Dedicated and not Elastic Premium,
# plans with SKU names starting with "P" won't scale dynamically and may increase your costs.
# from https://learn.microsoft.com/en-us/azure/azure-functions/functions-premium-plan?tabs=portal
