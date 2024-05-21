using CheckMade.Common.Interfaces.ExternalServices.GoogleApi;
using CheckMade.Common.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Tests.Integration;

public class GoogleApiTests
{
    private ServiceProvider? _services;
    private const string SkipReason = "Google Sheets not in use"; 
    
    [Fact(Skip = SkipReason)]
    public async Task GetAllSpreadsheetDataAsync_GetsAllData()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const string testSheetName = "tests_retrieve_all";
        var testSheetId = _services.GetRequiredService<UiSourceSheetIdProvider>().UiSourceSheetId;
        var sheetsService = _services.GetRequiredService<ISheetsService>();
        
        var expectedCells = new SheetData(new string[][]
        {
            // each row stops with the last value in it, and represents leading empty cell with empty string
            ["A1"],
            ["A2", "B2"],
            [string.Empty, string.Empty, "C3"]
        });
        
        // Act
        var actualCells = await sheetsService.GetAllSpreadsheetDataAsync(testSheetId, testSheetName);
        
        // Assert
        actualCells.Should().BeEquivalentTo(expectedCells);
    }

    [Fact(Skip = SkipReason)]
    public async Task GetSpreadsheetDataAsync_HandlesEscapedCharactersCorrectly()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const string testSheetName = "tests_special_char";
        var testSheetId = _services.GetRequiredService<UiSourceSheetIdProvider>().UiSourceSheetId;
        var sheetService = _services.GetRequiredService<ISheetsService>();

        var expectedCells = new SheetData(new string[][]
        {
            /* The escape character '\' below prevents the IDE's editor from interpreting it as a control char,
            and thus allows testing that it's part of what the GoogleSheets API returns.
            The second '\' in '\\n' tests for an actual line-break character being returned, rather than just the
            literal sequence of the characters '\n'.
            I.e. when the string is written out, the double quote and line break are converted to their final form! */
            ["'Enclosed by single quotes with leading excel-ignore quote'", 
                "\"Enclosed by double quotes\\n and manual linebreak\""],
        });
        
        // Act
        var actualCells = await sheetService.GetSpreadsheetDataAsync(
            testSheetId, "A1:B1", testSheetName);
        
        // Assert
        actualCells.Should().BeEquivalentTo(expectedCells);
    }

    [Fact(Skip = SkipReason)]
    public async Task GetSpreadsheetDataAsync_DoesNotTrimTrailingWhitespace()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const string testSheetName = "tests_special_char";
        var testSheetId = _services.GetRequiredService<UiSourceSheetIdProvider>().UiSourceSheetId;
        var sheetService = _services.GetRequiredService<ISheetsService>();

        var expectedCells = new SheetData(new string[][]
        {
            [" with trailing spaces that shouldn't be trimmed "]
        });
        
        // Act
        var actualCells = await sheetService.GetSpreadsheetDataAsync(
            testSheetId, "A2:A2", testSheetName);
        
        // Assert
        actualCells.Should().BeEquivalentTo(expectedCells);
    }
    
    [Fact(Skip = SkipReason)]
    public async Task GetSpreadsheetDataAsync_CorrectlyReturnsUnicodeCharacter()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const string testSheetName = "tests_special_char";
        var testSheetId = _services.GetRequiredService<UiSourceSheetIdProvider>().UiSourceSheetId;
        var sheetService = _services.GetRequiredService<ISheetsService>();

        var expectedCells = new SheetData(new string[][]
        {
            ["Unicode char: 😀"]
        });
        
        // Act
        var actualCells = await sheetService.GetSpreadsheetDataAsync(
            testSheetId, "C3:C3", testSheetName);
        
        // Assert
        actualCells.Should().BeEquivalentTo(expectedCells);
    }
    
    [Fact(Skip = SkipReason)]
    public async Task GetSpreadsheetDataAsync_ReturnsStringThatCanBeParameterized()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const string testSheetName = "tests_special_char";
        var testSheetId = _services.GetRequiredService<UiSourceSheetIdProvider>().UiSourceSheetId;
        var sheetService = _services.GetRequiredService<ISheetsService>();
        
        const string param1 = "param1";
        const string param2 = "param2";
        const string expected = $"With first value {param1} and second value {param2} inserted with string formatting.";
        
        // Act
        var cell = await sheetService.GetSpreadsheetDataAsync(
            testSheetId, "B2:B2", testSheetName);

        var actual = string.Format(cell.Cells[0][0], param1, param2);
        
        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}