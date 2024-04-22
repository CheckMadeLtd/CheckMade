# Permanently purge a KeyVault (beyond 'soft delete') - necessary to unlock/reset the use of secret key names (after they have been used before)
az keyvault purge --name VAULT_NAME

# Add a secret to the KeyVault:
az keyvault secret set --vault-name KEYVAULT_NAME --name SECRET_NAME --value SECRET
 
