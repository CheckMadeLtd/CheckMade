using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class TelegramFunctionTests(UnitTestStartup setup) : IClassFixture<UnitTestStartup>
{
    private readonly ServiceProvider _services = setup.GetServiceProvider();
    
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

        Assert.Throws<ArgumentNullException>(() => converter.ConvertMessage(telegramInputMessage));
    }

    [Fact]
    public void ConvertToModel_ReturnsCorrectModel_ForValidInput()
    {
        const long validUserId = 123L;
        const string validText = "Valid text message";
        var now = DateTime.Now;
        
        var telegramInputMessage = new Message
        {
            From = new User { Id = validUserId },
            Date = now,
            Text = validText
        };
        
        var expectedModel = new InputTextMessage(validUserId, new MessageDetails(validText, now));
        var converter = _services.GetRequiredService<IToModelConverter>();

        var actualModel = converter.ConvertMessage(telegramInputMessage);
        Assert.Equal(expectedModel, actualModel);
    }

    [Theory]
    [InlineData(BotType.Submissions)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleUpdateAsync_ReturnsEarlyForAnyBotType_WhenUpdateMessageIsNull(BotType botType)
    {
        var update = new Update { Message = null };

        var mockFactory = new Mock<IBotClientFactory>();
        var mockRequestProcessor = new Mock<IRequestProcessor>();
        var mockLogger = new Mock<ILogger<UpdateHandler>>();

        var converter = new Mock<IToModelConverter>().Object;
        var handler = new UpdateHandler(mockFactory.Object, mockRequestProcessor.Object, converter, mockLogger.Object);
        await handler.HandleUpdateAsync(update, botType);
        
        mockFactory.VerifyNoOtherCalls();
        mockRequestProcessor.VerifyNoOtherCalls();
    }
    
}