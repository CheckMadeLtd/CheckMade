using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class BotUpdateHandlerTests(UnitTestStartup setup) : IClassFixture<UnitTestStartup>
{
    private readonly ServiceProvider _services = setup.ServiceProvider;

    // Test with variety of values for validText
    [Fact]
    public async Task HandleUpdateAsync_ReturnsCorrectOutputMessage_ForValidUpdateToSubmissionsBot()
    {
        // Arrange
        const BotType botType = BotType.Submissions;
        const long validUserId = 123L;
        const long validChatId = 321L;
        const string validText = "Valid text message";
        var now = DateTime.Now;
        const string expectedOutputMessage = $"Echo: {validText}";

        var update = new Update
        {
            Message = new Message
            {
                From = new User { Id = validUserId },
                Chat = new Chat { Id = validChatId },
                Date = now,
                Text = validText
            }
        };

        var mockBotClientWrapper = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IBotUpdateHandler>();
        
        // Act
        var actualOutputMessage = await handler.HandleUpdateAsync(update, botType);
        
        // Assert
        actualOutputMessage.Should().Be(expectedOutputMessage);
        mockBotClientWrapper.Verify(x => x.SendTextMessageAsync(
                validChatId,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    
    [Theory]
    [InlineData(null, "Valid text")]
    [InlineData(123L, null)]
    [InlineData(null, null)]
    public void ConvertMessage_ThrowsArgumentNullException_ForInvalidInputs(long? userId, string text)
    {
        var converter = _services.GetRequiredService<IToModelConverter>();
        
        var telegramInputMessage = new Message
        {
            From = userId.HasValue ? new User { Id = userId.Value } : null,
            Text = text
        };
        
        Action convertMessage = () => converter.ConvertMessage(telegramInputMessage);
        convertMessage.Should().Throw<ArgumentNullException>();
    }
}