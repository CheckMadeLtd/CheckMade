using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;

namespace CheckMade.Services.ExtAPIs.GoogleApi;

public sealed class GoogleAuth(string credentialFile)
{
    public const string GglApiCredentialFileKey = "GOOGLE_API_CREDENTIAL_FILE";
    
    internal GoogleCredential CreateCredential() => 
        GoogleCredential.FromFile(credentialFile).CreateScoped(SheetsService.Scope.Spreadsheets);
}