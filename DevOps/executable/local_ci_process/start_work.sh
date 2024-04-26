#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

set -x # Tracing subsequent commands in the console output

git checkout main
git pull

set +x
# Generate a random 6-character alphanumeric string
random_string=$(openssl rand -base64 6 | tr -dc 'a-zA-Z0-9' | fold -w 6 | head -n 1)
new_temp_branch="tmp/$random_string"

set -x
git checkout -b "$new_temp_branch" # start work (including on unstaged changes) on a new temp branch
