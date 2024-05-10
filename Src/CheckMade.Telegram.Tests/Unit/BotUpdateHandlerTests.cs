using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class BotUpdateHandlerTests
{
    private ServiceProvider? _services;
    
    [Theory]
    [InlineData("_")]
    [InlineData("Normal valid text message")]
    [InlineData(" valid text message \n with line break and trailing spaces ")]
    public async Task HandleUpdateAsync_SendsCorrectOutputMessage_ForValidUpdateToSubmissionsBot(string inputText)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const BotType botType = BotType.Submissions;
        const long validUserId = 123L;
        const long validChatId = 321L;
        var now = DateTime.Now;

        var update = new Update
        {
            Message = new Message
            {
                From = new User { Id = validUserId },
                Chat = new Chat { Id = validChatId },
                Date = now,
                Text = inputText
            }
        };

        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        
        // Act
        await handler.HandleUpdateAsync(update, botType);
        
        // Assert
        var expectedOutputMessage = $"Echo: {inputText}";
        
        mockBotClientWrapper.Verify(x => x.SendTextMessageAsync(
                validChatId,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Fact]
    public async Task HandleUpdateAsync_ThrowsArgumentNullException_ForEmptyMessageToSubmissionsBot()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const BotType botType = BotType.Submissions;
        var update = new Update { Message = null };
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        
        // Act
        Func<Task> handleUpdate = () => handler.HandleUpdateAsync(update, botType);
        
        // Assert
        await handleUpdate.Should().ThrowAsync<ArgumentNullException>();
    }

    [Theory]
    [InlineData(BotType.Submissions)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleUpdateAsync_Fails_ForUpdateOfUnhandledType(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var update = new Update { CallbackQuery = new CallbackQuery() };
        
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        Func<Task> handleUpdate = () => handler.HandleUpdateAsync(update, botType);
        await handleUpdate.Should().ThrowAsync<Exception>();
    }
}
