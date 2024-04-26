#- use 
chmod +x yourscript.sh
#` to  give execution rights to the current user for this script (or 
chmod -R +x /path/to/directory
# for all scripts in a dir!!

#- To execute,
./yourscript.sh
# while being in the scripts dir (otherwise replace prepend the file name with `dir/dir/dir/` etc. with current dir as starting point
 

# Example for a looping menu

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
