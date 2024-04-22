#!/opt/homebrew/bin/bash

set -e 
set -o pipefail
# -------------------------------------------------------------------------------------------------------

# Assuming an Azuire Portal account and the CheckMade Subscription are already set up (but nothing else)

az login
az init # for some global, common preset config e.g. output format
az account set --subscription CheckMade
az configure --defaults location=uksouth
