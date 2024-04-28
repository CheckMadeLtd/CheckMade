#!/usr/bin/env bash
set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------

#  Using '>&2' for echo commands that are expected to print to stdout because all normal 'echo' is captured
#  by command substitution in the calling script. So we pragmatically abuse the stderr here to make the 
#  messages visible.

confirm_and_select_resource() {
    local resource_type="$1" # The type of resource (e.g., "functionapp")
    local current_value="$2"
    local confirm

    # Derive selection function name from resource_type
    local selection_function="select_${resource_type}"

    # Display the current selection (if any) and ask for confirmation
    if [ -n "$current_value" ]; then
        echo "Currently selected $resource_type: '$current_value'. Correct (y/n)?" >&2
        read -r confirm
        if [ "$confirm" != "y" ]; then
            current_value=$($selection_function)
            if [ -z "$current_value" ]; then
                echo "Selection is mandatory. Exiting." >&2
                exit 1
            fi
        fi
    else
        # No current selection, force a selection
        current_value=$($selection_function)
        if [ -z "$current_value" ]; then
            echo "Selection is mandatory. Exiting." >&2
            exit 0
        fi
    fi

    echo "'${current_value}' has been selected." >&2
    echo "$current_value" # Return the selected/confirmed value
}

select_functionapp() {
    local selected_functionapp
    selected_functionapp=$(select_azure_resource "functionapp" "az functionapp list") || {
        echo "Selection cancelled by user." >&2
        exit 0
    }
    echo "$selected_functionapp"
}

select_storage_account() {
    local selected_storage_account 
    selected_storage_account=$(select_azure_resource "storage account" "az storage account list") || {
        echo "Selection cancelled by user." >&2
        exit 0
    }
    echo "$selected_storage_account"
}

select_keyvault() {
    local selected_keyvault 
    selected_keyvault=$(select_azure_resource "keyvault" "az keyvault list") || {
        echo "Selection cancelled by user." >&2
        exit 0
    }
    echo "$selected_keyvault"
}

select_azure_resource() {
    local resource_type="$1"
    local list_command="$2"
    
    local resources 
    resources=$($list_command --query "[*].name" --output tsv)
    local count
    count=$(echo "$resources" | wc -l | tr -d '[:space:]')

    if [ "$count" -eq 1 ]; then
        resource_name=$(echo "$resources" | tr -d '[:space:]')
        echo "Just one $resource_type found, continuing with '${resource_name}'" >&2
    else
        echo "$resources" >&2
        echo "See above for existing ${resource_type}s. Enter the name of the $resource_type which you want to select. \
To quit, press 'q'." >&2
        read -r resource_name
        if [ "$resource_name" == "q" ]; then
            echo "Quitting $resource_type selection." >&2
            exit 0
        fi
    fi

    echo "$resource_name"
}
