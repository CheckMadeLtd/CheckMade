using CheckMade.Telegram.Function;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class UpdateServiceTests
{
    [Theory]
    [InlineData(null, "Valid text")]
    [InlineData(123L, null)]
    [InlineData(null, null)]
    public void ConvertToModel_ThrowsArgumentNullException_ForInvalidInputs(long? userId, string text)
    {
        var telegramInputMessage = new Message
        {
            From = userId.HasValue ? new User { Id = userId.Value } : null,
            Text = text
        };

        Assert.Throws<ArgumentNullException>(() => UpdateService.ConvertToModel(telegramInputMessage));
    }

    [Fact]
    public void ConvertToModel_ReturnsCorrectModel_ForValidInput()
    {
        const long validUserId = 123L;
        const string validText = "Valid text message";
        
        var telegramInputMessage = new Message { From = new User { Id = validUserId }, Text = validText };
        var expectedModel = new InputTextMessage(validUserId, new MessageDetails(validText));

        var actualModel = UpdateService.ConvertToModel(telegramInputMessage);
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
        var mockLogger = new Mock<ILogger<UpdateService>>();

        var updateService = new UpdateService(mockFactory.Object, mockRequestProcessor.Object, mockLogger.Object);
        await updateService.HandleUpdateAsync(update, botType);
        
        mockFactory.VerifyNoOtherCalls();
        mockRequestProcessor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleUpdateAsync_UsesItsDependenciesAsExpected_WhenUpdateMessageValid()
    {
        var botType = BotType.Submissions;
        
        const long validUserId = 123L;
        const string validText = "Valid text message";
        var validChatId = new ChatId(321L);

        var update = new Update
        {
            Message = new Message
            {
                From = new User { Id = validUserId },
                Chat= new Chat { Id = validChatId.Identifier!.Value },
                Text = validText
            }
        };
        
        var mockFactory = new Mock<IBotClientFactory>();
        var mockRequestProcessor = new Mock<IRequestProcessor>();
        var mockLogger = new Mock<ILogger<UpdateService>>();

        var mockBotClient = new Mock<ITelegramBotClientAdapter>();
        mockFactory.Setup(factory => factory.CreateBotClient(botType)).Returns(mockBotClient.Object);
        
        var updateService = new UpdateService(mockFactory.Object, mockRequestProcessor.Object, mockLogger.Object);
        await updateService.HandleUpdateAsync(update, botType);

        mockRequestProcessor.Verify(rp => 
            rp.EchoAsync(It.IsAny<InputTextMessage>()), Times.Once);
        
        mockFactory.Verify(f => f.CreateBotClient(botType), Times.Once);
            
        mockBotClient.Verify(bc => 
            bc.SendTextMessageAsync(It.IsAny<ChatId>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        mockFactory.VerifyNoOtherCalls();
        mockRequestProcessor.VerifyNoOtherCalls();
        mockBotClient.VerifyNoOtherCalls();
    }
    
}