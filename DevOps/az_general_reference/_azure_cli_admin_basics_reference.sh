# !!!!!!!!!!!!!!!!
# NOT FOR EXECUTION, just a repository of CLI commands for now!!
# !!!!!!!!!!!!!!!!

# Launch interactive CLI menu
az interactive

# list of relevant az subgroups for basic setup
az account
az appservice
az cosmosdb
az functionapp
az group
az identity (?)
az keyvault
az network (?)
az postgres (?) (vs. cosmosdb?)
az resource
az resourcemanagement (?)
az role (RBAC)
az storage
az vm

# list of relevant 'commands' for basic use of az
az configure
az find (A.I. robot)
az interactive
az login
az logout
az upgrade
az version

# account related
az account show
az account list
az account list-locations -o table # "Get current location names"
az account set -s {SUBSCR_ID} # Set default subscription (name or id)

# Default setting (e.g. setting resource group)
az configure --list-defaults
az configure --defaults group=NAME_OF_GROUP

# FunctionApp related
az functionapp plan list # "Get current functionapp plan"

# ResourceGroup related
az group create
az group delete -n NAME_OF_GROUP
az group export # Captures a resource group as a template

