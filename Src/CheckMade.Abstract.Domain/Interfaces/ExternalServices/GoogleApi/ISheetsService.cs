namespace CheckMade.Abstract.Domain.Interfaces.ExternalServices.GoogleApi;

public interface ISheetsService
{
    Task<SheetData> GetSpreadsheetDataAsync(string sheetId, string sheetRange, string? sheetName = null);
    Task<SheetData> GetAllSpreadsheetDataAsync(string sheetId, string? sheetName = null);
}