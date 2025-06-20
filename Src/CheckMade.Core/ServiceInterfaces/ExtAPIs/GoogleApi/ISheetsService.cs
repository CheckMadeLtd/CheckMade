namespace CheckMade.Core.ServiceInterfaces.ExtAPIs.GoogleApi;

public interface ISheetsService
{
    Task<SheetData> GetSpreadsheetDataAsync(string sheetId, string sheetRange, string? sheetName = null);
    Task<SheetData> GetAllSpreadsheetDataAsync(string sheetId, string? sheetName = null);
}