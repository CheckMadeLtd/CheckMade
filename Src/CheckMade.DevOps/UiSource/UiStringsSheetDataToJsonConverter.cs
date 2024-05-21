using CheckMade.Common.Interfaces.ExternalServices.GoogleApi;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.UiSource;

public class UiStringsSheetDataToJsonConverter(ISheetsService sheetsService, UiSourceSheetData uiSource)
{
    public JObject Convert(SheetData uiStrings)
    {
        // var headers = uiSource.Headers;
        
        var appScope = new JObject();
        var telegramScope = new JObject();
        appScope["telegram"] = telegramScope;

        return appScope;
    }
}