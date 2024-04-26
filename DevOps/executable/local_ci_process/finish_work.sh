#!/opt/homebrew/bin/bash
set -e 
set -o pipefail

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

echo "Now running local Build & Tests for all Debug configurations contained in the *.sln file of this working dir," \
"except those excluded via --options. This ensures integrity before attempted merge into main (especially in case the" \
"current branch was rebased on a newer origin/main in the previous step)."

# Initialize flags for preventing ios or ios and android builds
noios=false
nomob=false

# Process options
while [[ $# -gt 0 ]]; do
    case "$1" in
        --noios)
            echo "--noios option active: projects containing 'iOS' in their name will be skipped."
            noios=true
            shift # Remove this option from processing (= SHIFT positional arguments to the left)
            ;;
        --nomob)
            echo "--nomob option active: projects containing 'iOS' or 'Android' in their name will be skipped."
            nomob=true
            shift
            ;;
        *)
            # Unknown option
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# Find the first .sln file in the current directory
sln_file=$(find . -maxdepth 1 -name "*.sln" | head -n 1)

if [ -z "$sln_file" ]; then
    echo "No .sln file found in the current directory."
    exit 1
fi

# Extract configurations starting with 'Debug_' from the .sln file
# We are building and testing selected 'Debug' builds here on the local dev machine, as recommended, for better tracability etc. 
# On the GitHub Action Runner / main workflow we then build and test the targeted 'Release' configuration for more realistic final tests. 
configurations=$(ggrep -P '^\s*Debug_' "$sln_file" | awk -F'|' '{print $1}' | sed 's/^[[:blank:]]*//;s/[[:blank:]]*$//')

configArray=()

shopt -s nocasematch  # Set shell option to ignore case in pattern matching

while IFS= read -r line; do
    if [[ "$noios" == true && $line =~ ios ]]; then
        echo "Skipping iOS build configuration: $line"
    elif [[ "$nomob" == true && ($line =~ ios || $line =~ android) ]]; then
        echo "Skipping mobile build configuration: $line"
    elif [[ $line =~ _all ]]; then
        echo "Skipping build configuration: $line"
    else
        configArray+=("$line")
    fi
done <<< "$configurations"

shopt -u nocasematch  # Unset the nocasematch option to return to default behavior

echo "The following Debug configurations have been found in the local Solution:"
for config in "${configArray[@]}"
do
    echo "$config"
done

set -x # Tracing subsequent commands in the console output

for config in "${configArray[@]}"
do
    dotnet restore /p:Configuration=$config
    dotnet build --configuration $config --no-restore --verbosity minimal
    dotnet test --no-build --configuration $config --verbosity minimal
done

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
        
        # Find the first csproj file with "android" in its name
        csproj_file=$(find . -iname "*android*.csproj" | head -n 1)
        if [ ! -z "$csproj_file" ]; then
            perl -i -pe 's/(<ApplicationVersion>)(\d+)(<\/ApplicationVersion>)/"$1".($2+1)."$3"/ge' "$csproj_file"
            echo "<ApplicationVersion> in the Android .csproj file was incremented by 1. This is the 'version code' for Google Play which needs to be an integer."
            perl -i -pe "s|<ApplicationDisplayVersion>.*</ApplicationDisplayVersion>|<ApplicationDisplayVersion>$new_version</ApplicationDisplayVersion>|g" "$csproj_file"
            echo "<ApplicationDisplayVersion> in the Android .csproj was updated to $new_version"
        fi

        git add .
        git commit -m "Script updates version to $new_version"
        break
    else
        echo "Invalid version format. Please follow the x.y.z format (integers only)."
    fi
done

echo "--------------------------------------------------------------------------"

while true; do
    echo "Type name (at least 2 chars) for this new feature branch (the prefix 'fb/' will be added automatically)." \
    "To abort, enter 'q': "

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
  echo "Checked out to new branch '$full_branch_name'. Ready to push to origin and trigger remote CI/CD workflow?" \
  "'y' or any other input to abort."
  
  read -r ready_to_push
  
  if [ "$ready_to_push" = "y" ]; then
    git push origin "$full_branch_name"
    echo "IMPORTANT: Confirm successful merger into main branch on GitHub, before continuing with the 'start_work' script!"
    break    
  else
    echo "Not pushing at this time."
    exit 0
  fi 
done

