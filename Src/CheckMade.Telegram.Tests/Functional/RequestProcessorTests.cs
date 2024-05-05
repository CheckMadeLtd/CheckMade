using CheckMade.Telegram.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Telegram.Tests.Functional;

public class RequestProcessorTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;
    
    [Fact]
    public void Echo_ReturnsEcho_WhenInputValid()
    {
        var fakeInputMessage = TestUtils.GetValidTelegramTestMessage(); 
        var expectedOutputText = $"Echo: {fakeInputMessage.Text}";
        var requestProcessor = _services.GetRequiredService<IRequestProcessor>();
        
        var actualOutputText = requestProcessor.Echo(fakeInputMessage.From!.Id, fakeInputMessage.Text!);

        actualOutputText.Should().Be(expectedOutputText);
    }
}
