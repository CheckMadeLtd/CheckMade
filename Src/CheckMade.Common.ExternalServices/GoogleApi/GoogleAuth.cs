using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;

namespace CheckMade.Common.ExternalServices.GoogleApi;

public class GoogleAuth(string credentialFile)
{
    internal GoogleCredential CreateCredential() => 
        GoogleCredential.FromFile(credentialFile).CreateScoped(SheetsService.Scope.Spreadsheets);
}