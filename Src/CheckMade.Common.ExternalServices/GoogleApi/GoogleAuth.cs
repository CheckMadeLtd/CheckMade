using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;

namespace CheckMade.Common.ExternalServices.GoogleApi;

public class GoogleAuth(string credentialFile)
{
    public const string GglApiCredentialFileKey = "GOOGLE_API_CREDENTIAL_FILE";
    
    internal GoogleCredential CreateCredential() => 
        GoogleCredential.FromFile(credentialFile).CreateScoped(SheetsService.Scope.Spreadsheets);
}