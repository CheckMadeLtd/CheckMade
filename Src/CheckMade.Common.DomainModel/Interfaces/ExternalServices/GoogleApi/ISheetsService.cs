namespace CheckMade.Common.DomainModel.Interfaces.ExternalServices.GoogleApi;

public interface ISheetsService
{
    Task<SheetData> GetSpreadsheetDataAsync(string sheetId, string sheetRange, string? sheetName = null);
    Task<SheetData> GetAllSpreadsheetDataAsync(string sheetId, string? sheetName = null);
}