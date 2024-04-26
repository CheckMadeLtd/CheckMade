#!/opt/homebrew/bin/bash

set -e 
set -o pipefail

# -------------------------------------------------------------------------------------------------------


confirm_script_launch() {
    local script_path="$1"
    local first_argument="$2"
    
    echo "Launch $script_path (y/n)?"
    read -r confirm_launch
    if [ "$confirm_launch" == "y" ]; then
      if [ -z "$first_argument" ]; then
          source "$script_path"
      else
          source "$script_path" "$first_argument"
      fi
    fi
}

confirm_command() {
    local confirm_msg="$1"
    local command="$2"

    echo $confirm_msg
    read -r answer
    if [ "$answer" == "y" ]; then
        eval "$command"
    fi
}
