using CheckMade.Common.Interfaces.ExternalServices.GoogleApi;
using CheckMade.Common.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Common.Tests.Integration;

public class GoogleApiTests
{
    private ServiceProvider? _services;

    [Fact]
    public async Task GetAllSpreadsheetDataAsync_GetsAllData()
    {
        _services = new IntegrationTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const string testSheetId = "1b6AHy35omBwmUsMNIfRjRIEJ__4YxLnNwviz8h8287I";
        const string testSheetName = "tests_retrieve_all";
        var sheetsService = _services.GetRequiredService<ISheetsService>();
        var actualCells = await sheetsService.GetAllSpreadsheetDataAsync(testSheetId, testSheetName);
        
        var expectedCells = new SheetData(new string[][]
        {
            ["A1", "B1", string.Empty],
            ["A2", "B2", string.Empty],
            [string.Empty, string.Empty, "C3"]
        });
    }
}