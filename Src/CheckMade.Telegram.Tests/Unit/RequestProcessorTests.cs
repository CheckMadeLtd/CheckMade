using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Unit;

public class RequestProcessorTests(UnitTestStartup setup) : IClassFixture<UnitTestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;
    
    [Fact]
    public async Task Echo_ReturnsEcho_WhenInputValid()
    {
        var fakeInputMessage = TestUtils.GetValidTestMessage(); 
        var expectedOutputText = $"Echo: {fakeInputMessage.Details.Text}";
        var requestProcessor = _services.GetRequiredService<IRequestProcessor>();
        
        var actualOutputText = await requestProcessor.EchoAsync(fakeInputMessage);

        actualOutputText.Should().Be(expectedOutputText);
    }
}
