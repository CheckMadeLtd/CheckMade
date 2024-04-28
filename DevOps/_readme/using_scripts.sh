# Naming convention for variables
# -- local vars in functions and vars only used within a single script
lower_caps
# -- global vars (used across scripts in the same shellenv (e.g. in sourced sub-scripts)) or ENVIRONMENT variables
ALL_CAPS

# To make a variable available in child shellenvs use 'export [var]'

# To  give execution rights to the current user for this script or:
chmod +x yourscript.sh
# For all scripts in a dir!! 
chmod -R +x /path/to/directory
# To execute in a new shellenv
./yourscript.sh
# To execute in the current shellenv:
source ./yourscript.sh
or
. ./yourscript.sh


# Example for an interactive menu with a looping menu
while true; do
  echo "What would you like to do? 
  'l' for list of location names;
  'p' for current function plan;
  'q' to quit."
  read answer

  if [ "$answer" == "l" ]; then
    echo "Get current location names"
    az account list-locations -o table
  elif [ "$answer" == "p" ]; then
    echo "Get current functionapp plan"
    az functionapp plan list
  elif [ "$answer" == "q" ]; then
    break
  else
    echo "Invalid input. Please try again."
  fi
done
