using CheckMade.Common.Interfaces.ExternalServices.GoogleApi;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Startup.ConfigProviders;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Tests.Integration;

public class GoogleApiTests
{
    private ServiceProvider? _services;
    private const string SkipReason = "Google Sheets currently not in use -> reducing test execution time"; 
    
    [Fact(Skip = SkipReason)]
    // [Fact]
    public async Task GetAllSpreadsheetDataAsync_GetsAllData()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string testSheetName = "tests_retrieve_all";
        
        var expectedCells = new SheetData(new string[][]
        {
            // each row stops with the last value in it, and represents leading empty cell with empty string
            ["A1"],
            ["A2", "B2"],
            [string.Empty, string.Empty, "C3"]
        });
        
        var actualCells = 
            await basics.sheetsService.GetAllSpreadsheetDataAsync(basics.testSheetId, testSheetName);
        
        Assert.Equivalent(expectedCells, actualCells);
    }

    [Fact(Skip = SkipReason)]
    // [Fact]
    public async Task GetSpreadsheetDataAsync_HandlesEscapedCharactersCorrectly()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string testSheetName = "tests_special_char";

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
        
        var actualCells = await basics.sheetsService.GetSpreadsheetDataAsync(
            basics.testSheetId, "A1:B1", testSheetName);
        
        Assert.Equivalent(expectedCells, actualCells);
    }

    [Fact(Skip = SkipReason)]
    // [Fact]
    public async Task GetSpreadsheetDataAsync_DoesNotTrimTrailingWhitespace()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string testSheetName = "tests_special_char";

        var expectedCells = new SheetData(new string[][]
        {
            [" with trailing spaces that shouldn't be trimmed "]
        });
        
        var actualCells = await basics.sheetsService.GetSpreadsheetDataAsync(
            basics.testSheetId, "A2:A2", testSheetName);
        
        Assert.Equivalent(expectedCells, actualCells);
    }
    
    [Fact(Skip = SkipReason)]
    // [Fact]
    public async Task GetSpreadsheetDataAsync_CorrectlyReturnsUnicodeCharacter()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string testSheetName = "tests_special_char";

        var expectedCells = new SheetData(new string[][]
        {
            ["Unicode char: 😀"]
        });
        
        var actualCells = await basics.sheetsService.GetSpreadsheetDataAsync(
            basics.testSheetId, "C3:C3", testSheetName);
        
        Assert.Equivalent(expectedCells, actualCells);
    }
    
    [Fact(Skip = SkipReason)]
    // [Fact]
    public async Task GetSpreadsheetDataAsync_ReturnsStringThatCanBeParameterized()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string testSheetName = "tests_special_char";
        const string param1 = "param1";
        const string param2 = "param2";
        const string expected = $"With first value {param1} and second value {param2} inserted with string formatting.";
        
        var cell = await basics.sheetsService.GetSpreadsheetDataAsync(
            basics.testSheetId, "B2:B2", testSheetName);

        var actual = string.Format(cell.Cells[0][0], param1, param2);
        
        Assert.Equal(expected, actual);
    }

    private (string testSheetId, ISheetsService sheetsService) GetBasicTestingServices(IServiceProvider sp) =>
        (testSheetId: sp.GetRequiredService<UiSourceSheetIdProvider>().UiSourceSheetId,
            sheetsService: sp.GetRequiredService<ISheetsService>());
}