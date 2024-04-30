#!/usr/bin/env bash

set -e 
set -o pipefail

source "$(dirname "${BASH_SOURCE[0]}")/../../global_utils.sh"

# -------------------------------------------------------------------------------------------------------

# Assuming an Azure Portal account and the CheckMade Subscription are already set up (but nothing else)

az login

confirm_command \
"Launch az init for some global, common az preset config e.g. output format?" \
"az init"

az account set --subscription CheckMade
az configure --defaults location=uksouth
