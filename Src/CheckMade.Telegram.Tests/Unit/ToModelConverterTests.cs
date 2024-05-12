using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class ToModelConverterTests
{
    private ServiceProvider? _services;

    // [Fact]
    // public void ConvertMessage_SavesPhotoCaptionAsText_ForValidPhotoMessageWithCaption()
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     
    //     // Arrange
    //     var utils = _services.GetRequiredService<ITestUtils>();
    //     var photoMessage = utils.GetValidPhotoMessage();
    //     var converter = _services.GetRequiredService<IToModelConverter>();
    //     
    //     // Act 
    // }
    
    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenUserIsNull()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var message = new Message { From = null, Text = "not empty" };
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converter = _services.GetRequiredService<IToModelConverter>();
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageAsync(message, mockBotClient.Object);

        // Assert
        convertMessage.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenTextAndAttachmentFileIdBothEmpty()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var message = new Message { From = new User { Id = 123L } };
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converter = _services.GetRequiredService<IToModelConverter>();
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageAsync(message, mockBotClient.Object);

        // Assert
        convertMessage.Should().ThrowAsync<ArgumentNullException>();
    }
}