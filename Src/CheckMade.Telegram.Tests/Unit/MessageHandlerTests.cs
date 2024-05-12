using CheckMade.Common.Utils;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace CheckMade.Telegram.Tests.Unit;

public class MessageHandlerTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData(BotType.Submissions)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleMessageAsync_SendsCorrectEchoMessageByBotType_ForValidTextMessage(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var textMessage = utils.GetValidTextMessage("simple valid text");
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var expectedOutputMessage = $"Echo from bot {botType}: {textMessage.Text}";

        // Act
        await handler.HandleMessageAsync(textMessage, botType);
        
        // Assert
        mockBotClient.Verify(x => x.SendTextMessageAsync(
                textMessage.Chat.Id,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    [InlineData(AttachmentType.Video)]
    public async Task HandleMessageAsync_SendsCorrectEchoMessage_ForValidAttachmentMessageToSubmissions(
        AttachmentType type)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var attachmentMessage = type switch
        {
            AttachmentType.Audio => utils.GetValidAudioMessage(),
            AttachmentType.Document => utils.GetValidDocumentMessage(),
            AttachmentType.Photo => utils.GetValidPhotoMessage(),
            AttachmentType.Video => utils.GetValidVideoMessage(),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var expectedOutputMessage = $"Echo from bot Submissions: {type}";
        
        // Act
        await handler.HandleMessageAsync(attachmentMessage, BotType.Submissions);
        
        // Assert
        mockBotClient.Verify(x => x.SendTextMessageAsync(
                attachmentMessage.Chat.Id,
                expectedOutputMessage, 
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task HandleMessageAsync_LogsWarningAndReturns_ForUnhandledMessageType()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        var unknownMessage = new Message // type 'Unknown' is derived by Telegram for lack of any props!
        {
            Chat = new Chat { Id = 123L }
        };

        var expectedLoggedMessage = $"Received message of type '{MessageType.Unknown}': " +
                                    $"{BotUpdateSwitch.NoSpecialHandlingWarningMessage}";

        var mockLogger = new Mock<ILogger<MessageHandler>>();
        mockLogger.Setup(l => l.Log(
                LogLevel.Warning, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLoggedMessage)), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception, string>>()!))
            .Verifiable();
        serviceCollection.AddScoped<ILogger<MessageHandler>>(_ => mockLogger.Object);
        
        _services = serviceCollection.BuildServiceProvider();
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act 
        await handler.HandleMessageAsync(unknownMessage, BotType.Submissions);
        
        // Assert
        mockLogger.Verify();
    }
    
    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task HandleMessageAsync_OutputsCorrectErrorMessage_WhenDataAccessExceptionThrown()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        var mockSubmissionsRequestProcessor = new Mock<ISubmissionsRequestProcessor>();
        mockSubmissionsRequestProcessor
            .Setup<Task<string>>(rp => rp.EchoAsync(It.IsAny<InputMessage>()))
            .Throws(new DataAccessException("Mock DataAccess Error", new Exception()));

        var mockRequestProcessorSelector = new Mock<IRequestProcessorSelector>();
        mockRequestProcessorSelector
            .Setup(rps => rps.GetRequestProcessor(BotType.Submissions))
            .Returns(mockSubmissionsRequestProcessor.Object);
        
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => mockRequestProcessorSelector.Object);
        
        _services = serviceCollection.BuildServiceProvider();

        const string expectedErrorMessage = $"{MessageHandler.DataAccessExceptionErrorMessageStub} " +
                                            $"{MessageHandler.CallToActionMessageAfterErrorReport}";
        
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        
        mockBotClient
            .Setup(x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), 
                expectedErrorMessage, 
                It.IsAny<CancellationToken>()))
            .Verifiable();

        var utils = _services.GetRequiredService<ITestUtils>();
        var textMessage = utils.GetValidTextMessage("some valid text");
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act 
        await handler.HandleMessageAsync(textMessage, BotType.Submissions);
        
        // Assert
        mockBotClient.Verify();
    }
}
