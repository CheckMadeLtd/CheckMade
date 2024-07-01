using CheckMade.Common.Interfaces.ExternalServices.GoogleApi;

namespace CheckMade.DevOps.UiSource;

public class UiSourceSheetData(ISheetsService sheetsService, UiSourceSheetIdProvider idProvider)
{
    private const string UiSourceSheetName = "ui_strings";

    public async Task<string[]> GetHeadersAsync() =>
        (await sheetsService
            .GetSpreadsheetDataAsync(idProvider.UiSourceSheetId, "A1:F1", UiSourceSheetName))
        .Cells[0];

    public async Task<SheetData> GetDataAsync()
    {
        var entireSheet = await sheetsService.GetAllSpreadsheetDataAsync(idProvider.UiSourceSheetId, UiSourceSheetName);
        var sheetWithoutHeader = entireSheet.Cells[1..];

        return new SheetData(sheetWithoutHeader);
    }
}