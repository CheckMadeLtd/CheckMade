using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class ToModelConverterTests
{
    private ServiceProvider? _services;

    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenUserIsNull()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var message = new Message { From = null, Text = "not empty" };
        var converter = _services.GetRequiredService<IToModelConverter>();
        
        // Act
        Action convertMessage = () => converter.ConvertMessage(message);

        // Assert
        convertMessage.Should().Throw<ArgumentNullException>();
    }
    
    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenTextIsEmpty()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var message = new Message { From = new User { Id = 123L }, Text = " " };
        var converter = _services.GetRequiredService<IToModelConverter>();
        
        // Act
        Action convertMessage = () => converter.ConvertMessage(message);

        // Assert
        convertMessage.Should().Throw<ArgumentNullException>();
    }
}