using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Functional;

public class RequestProcessorTests(FunctionalTestStartup setup) : IClassFixture<FunctionalTestStartup>
{
    private readonly ServiceProvider _services = setup.GetServiceProvider();
    
    [Fact]
    public void Echo_ReturnsEcho_WhenInputValid()
    {
        var fakeInputMessage = TestUtils.GetValidTestMessage(); 
        var expectedOutputText = $"Echo v0.6.1: {fakeInputMessage.Details.Text}";
        var requestProcessor = _services.GetRequiredService<IRequestProcessor>();
        
        var actualOutputText = requestProcessor.Echo(fakeInputMessage);

        actualOutputText.Should().Be(expectedOutputText);
    }
}
