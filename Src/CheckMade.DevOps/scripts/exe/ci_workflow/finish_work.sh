#!/usr/bin/env bash
set -e 
set -o pipefail
script_dir_finishwork=$(dirname "${BASH_SOURCE[0]}")
source "$script_dir_finishwork/../script_utils.sh"

# -------------------------------------------------------------------------------------------------------

working_dir_is_solution_root

echo "--------------------------------------------------------------------------"

echo "Current branch: $(git branch --show-current)"
echo "Confirm that you are checked out on the correct tmp/dev branch and that you deem it ready" \
"for a merge attempt into origin/main ('y' / any other for abort):"

read -r ready_for_merge_attempt
if [ "$ready_for_merge_attempt" != "y" ]; then
  echo "Operation aborted."
  exit 0
fi

echo "--------------------------------------------------------------------------"

echo "Now checking if origin/main has new commits since you checked out from it."

git fetch origin # Updates all local remote-tracking branches (incl. origin/main)
if git merge-base --is-ancestor origin/main HEAD; then
    echo "No new commits on origin/main since you checked out from it."
else
    echo "origin/main branch has new commits which will now be applied to your current branch. " \
    # See this chat for explanation why 'rebase' instead of 'merge' in this case:
    # https://chat.openai.com/share/e0ea368e-4457-482b-9a7c-da1bf4fa7fac
    git rebase origin/main
fi

echo "--------------------------------------------------------------------------"

# Once we add Desktop client, update this to a configuration that includes it.
config="Debug_Bot"

echo "Now running local Build & Tests for the $config configuration." \
"This ensures integrity before attempted merge into main (especially in case the" \
"current branch was rebased on a newer origin/main in the previous step)." \
"FYI: On the GitHub Action Runner / main workflow we will later build and test the targeted 'Release' configuration" \
"for more realistic final tests."

set -x # Tracing subsequent commands in the console output

dotnet restore /p:Configuration="$config"
dotnet build --configuration "$config" --no-restore --verbosity minimal
# Only unit tests due to mysterious fatal crashes of 'dotnet test' with some integration tests
dotnet test --no-build --configuration "$config" --verbosity minimal --filter FullyQualifiedName~CheckMade.Tests.Unit

set +x # Stop tracing

echo "--------------------------------------------------------------------------"

echo "Current version: $(cat version.txt)"

while true; do
    echo "Type new version number following semantic versioning rules (x.y.z). To abort, enter 'q':"

    read -r new_version

    if [ "$new_version" = "q" ]; then
        echo "Operation aborted."
        exit 0
    elif echo "$new_version" | grep -Eq '^[0-9]+\.[0-9]+\.[0-9]+$'; then
        echo "$new_version" > version.txt
        echo "The version.txt was updated with $new_version"
        
        git add .
        git commit -m "Script updates version to $new_version"
        break
    else
        echo "Invalid version format. Please follow the x.y.z format (integers only)."
    fi
done

echo "--------------------------------------------------------------------------"

while true; do
    echo "Type name (at least 2 chars) for this new feature branch (the prefix 'fb/' will be added automatically). \
To abort, enter 'q': "

    read -r branch_name

    if [ "$branch_name" = "q" ]; then
        echo "Operation aborted."
        exit 0
    elif [ ${#branch_name} -lt 2 ]; then
        echo "Error: The name must be at least 2 characters long."
    else
        full_branch_name="fb/$branch_name"
        break
    fi
done

git checkout -b "$full_branch_name"

echo "--------------------------------------------------------------------------"

while true; do
  echo "Checked out to new branch '$full_branch_name'. Ready to push to origin and trigger remote CI/CD workflow? \
'y' or any other input to abort."
  
  read -r ready_to_push
  
  if [ "$ready_to_push" = "y" ]; then
    git push origin "$full_branch_name"
    echo "IMPORTANT: Confirm successful merger into main on GitHub, before continuing with the 'start_work' script!"
    break    
  else
    echo "Not pushing at this time."
    exit 0
  fi 
done
