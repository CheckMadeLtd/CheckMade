using CheckMade.Common.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Chat.Tests;

public class RequestProcessorTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private const string DependencyFailMessage = "The dependency of this type failed to resolve";
    private readonly ServiceProvider _provider = setup.ServiceProvider;

    [Fact]
    public void Echo_ReturnsEcho_WhenInputValid()
    {
        const long fakeTelegramUserId = 123;
        const string fakeInputText = "Hello, world!";
        var expectedOutputText = $"Echo: {fakeInputText}";

        var requestProcessor = _provider.GetService<IRequestProcessor>() 
                                ?? throw new ArgumentNullException(nameof(IRequestProcessor), DependencyFailMessage);
        
        var actualOutputText = requestProcessor.Echo(fakeTelegramUserId, fakeInputText);

        actualOutputText.Should().Be(expectedOutputText);
    }
}