using CheckMade.Interfaces;
using FluentAssertions;

namespace CheckMade.Tests;

public class ResponseGeneratorTests(IResponseGenerator responseGenerator)
{
    [Fact]
    public void Echo_ReturnsEcho_WhenInputValid()
    {
        const string fakeInputText = "Hello, world!";
        var expectedOutputText = $"Echo: {fakeInputText}";

        var actualOutputText = responseGenerator.Echo(fakeInputText);

        actualOutputText.Should().Be(expectedOutputText);
    }
}