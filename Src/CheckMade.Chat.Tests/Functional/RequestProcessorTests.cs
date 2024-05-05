using CheckMade.Common.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Chat.Tests.Functional;

public class RequestProcessorTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;

    [Fact]
    public void Echo_ReturnsEcho_WhenInputValid()
    {
        const long fakeTelegramUserId = 123;
        const string fakeInputText = "Hello, world!";
        var expectedOutputText = $"Echo: {fakeInputText}";

        var requestProcessor = _services.GetRequiredService<IRequestProcessor>();
        
        var actualOutputText = requestProcessor.Echo(fakeTelegramUserId, fakeInputText);

        actualOutputText.Should().Be(expectedOutputText);
    }
}