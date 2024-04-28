#!/opt/homebrew/bin/bash
set -e 
set -o pipefail
script_dir_startwork=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir_startwork/../global_utils.sh"

# -------------------------------------------------------------------------------------------------------

working_dir_is_solution_root

set -x

git checkout main
git pull

new_temp_branch="tmp/$(get_random_id)"
git checkout -b "$new_temp_branch" # start work (including on unstaged changes) on a new temp branch

set +x