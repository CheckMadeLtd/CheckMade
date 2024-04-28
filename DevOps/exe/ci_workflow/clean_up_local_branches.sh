#!/usr/bin/env bash
set -e 
set -o pipefail
script_dir_cleanup=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir_cleanup/../global_utils.sh"

# -------------------------------------------------------------------------------------------------------

working_dir_is_solution_root

current_branch=$(git branch --show-current)
echo "Current branch is '$current_branch'."

# Create a temporary file to hold branch names
temp_file=$(mktemp)
# Ensure the temporary file is deleted on exit or if the script is interrupted
trap 'rm -f "$temp_file"' EXIT

# Write branch names to the temporary file, excluding the current and main branch
git branch | grep -v "^\*" | grep -v "^  main" > "$temp_file"

while IFS= read -r branch; do
    # Trim leading spaces
    branch=$(echo "$branch" | xargs)
    
    echo "Do you want to force-delete (!!!) the branch '$branch'? (y/n)"
    read -r confirm < /dev/tty
    if [[ $confirm == [yY] ]]; then
        git branch -D "$branch"
    fi
done < "$temp_file"

echo "Branch cleanup complete."
