using CheckMade.Common.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.Chat.Tests;

public class ResponseGeneratorTests(TestStartup setup) : IClassFixture<TestStartup>
{
    private const string ProviderNotInitializedMessage = "The Services Provider is null, failed to initialize";
    private readonly ServiceProvider _provider = setup.ServiceProvider;

    [Fact]
    public void Echo_ReturnsEcho_WhenInputValid()
    {
        const string fakeInputText = "Hello, world!";
        var expectedOutputText = $"Echo: {fakeInputText}";

        var responseGenerator = _provider.GetService<IResponseGenerator>() 
                                ?? throw new InvalidOperationException(ProviderNotInitializedMessage);
        var actualOutputText = responseGenerator.Echo(fakeInputText);

        actualOutputText.Should().Be(expectedOutputText);
    }
}