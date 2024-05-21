using CheckMade.Common.Interfaces.ExternalServices.GoogleApi;
using CheckMade.Common.Tests.Startup;
using FluentAssertions;
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
}