using CheckMade.Common.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Chat.Tests;

public class RequestProcessorTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private const string ProviderNotInitializedMessage = "The Services Provider is null, failed to initialize";
    private readonly ServiceProvider _provider = setup.ServiceProvider;

    [Fact]
    public void Echo_ReturnsEcho_WhenInputValid()
    {
        const long fakeTelegramUserId = 123;
        const string fakeInputText = "Hello, world!";
        var expectedOutputText = $"Echo: {fakeInputText}";

        var requestProcessor = _provider.GetService<IRequestProcessor>() 
                                ?? throw new InvalidOperationException(ProviderNotInitializedMessage);
        var actualOutputText = requestProcessor.Echo(fakeTelegramUserId, fakeInputText);

        actualOutputText.Should().Be(expectedOutputText);
    }
}