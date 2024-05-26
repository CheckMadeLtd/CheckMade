using CheckMade.Common.LangExt;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Xunit.Abstractions;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace CheckMade.Tests.Unit.Telegram;

public class MessageHandlerTests(ITestOutputHelper outputHelper)
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
        var textMessage = utils.GetValidTelegramTextMessage("simple valid text");
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var expectedOutputMessage = $"Echo from bot {botType}: {textMessage.Text}";

        // Act
        await handler.SafelyHandleMessageAsync(textMessage, botType);
        
        // Assert
        mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
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
        AttachmentType attachmentType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var attachmentMessage = attachmentType switch
        {
            AttachmentType.Audio => utils.GetValidTelegramAudioMessage(),
            AttachmentType.Document => utils.GetValidTelegramDocumentMessage(),
            AttachmentType.Photo => utils.GetValidTelegramPhotoMessage(),
            AttachmentType.Video => utils.GetValidTelegramVideoMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var expectedOutputMessage = $"Echo from bot Submissions: {attachmentType}";
        
        // Act
        await handler.SafelyHandleMessageAsync(attachmentMessage, BotType.Submissions);
        
        // Assert
        mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
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
                                    $"{BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish()}";

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
        await handler.SafelyHandleMessageAsync(unknownMessage, BotType.Submissions);
        
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
        const string mockErrorMessage = "Mock DataAccess Error";
        
        mockSubmissionsRequestProcessor
            .Setup<Task<Attempt<UiString>>>(rp => 
                rp.SafelyEchoAsync(It.IsAny<InputMessage>()))
            .Returns(Task.FromResult(Attempt<UiString>
                .Fail(new Failure(new DataAccessException(mockErrorMessage, new Exception())))));

        var mockRequestProcessorSelector = new Mock<IRequestProcessorSelector>();
        mockRequestProcessorSelector
            .Setup(rps => rps.GetRequestProcessor(BotType.Submissions))
            .Returns(mockSubmissionsRequestProcessor.Object);
        
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => mockRequestProcessorSelector.Object);
        
        _services = serviceCollection.BuildServiceProvider();
        
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        
        mockBotClient
            .Setup(x => x.SendTextMessageOrThrowAsync(
                It.IsAny<ChatId>(), 
                It.Is<string>(output => output.Contains(mockErrorMessage)), 
                It.IsAny<CancellationToken>()))
            .Verifiable();

        var utils = _services.GetRequiredService<ITestUtils>();
        var textMessage = utils.GetValidTelegramTextMessage("random valid text");
        var handler = _services.GetRequiredService<IMessageHandler>();
        
        // Act 
        await handler.SafelyHandleMessageAsync(textMessage, BotType.Submissions);
        
        // Assert
        mockBotClient.Verify();
    }

    [Fact]
    public async Task HandleMessageAsync_EchosCorrectBotCommand_ForValidBotCommandInputToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var validBotCommand = new BotCommandMenus()
            .SubmissionsBotCommandMenu[SubmissionsBotCommands.Problem].Command.RawEnglishText;
        var utils = _services.GetRequiredService<ITestUtils>();
        var botCommandMessage = utils.GetBotCommandMessage(validBotCommand);
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var expectedOutputMessage = $"Echo of a Submissions BotCommand: {validBotCommand}";

        // Act
        await handler.SafelyHandleMessageAsync(botCommandMessage, BotType.Submissions);

        // Assert
        mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
                botCommandMessage.Chat.Id,
                expectedOutputMessage,
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShowsCorrectError_ForInvalidBotCommandToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        const string invalidBotCommand = "/invalid";
        var utils = _services.GetRequiredService<ITestUtils>();
        var invalidBotCommandMessage = utils.GetBotCommandMessage(invalidBotCommand);
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        const string expectedErrorCode = "W3DL9";
    
        mockBotClient
            .Setup(
                x => x.SendTextMessageOrThrowAsync(
                    invalidBotCommandMessage.Chat.Id,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, CancellationToken>((_, msg, _) => 
                outputHelper.WriteLine(msg));
        
        // Act
        await handler.SafelyHandleMessageAsync(invalidBotCommandMessage, BotType.Submissions);
    
        // Assert
        mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                invalidBotCommandMessage.Chat.Id,
                It.Is<string>(msg => msg.Contains(expectedErrorCode)),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Theory]
    [InlineData(BotType.Submissions)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleMessageAsync_ShowsCorrectWelcomeMessage_UponStartCommand(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var handler = _services.GetRequiredService<IMessageHandler>();
        var startCommandMessage = utils.GetBotCommandMessage(Start.Command);
        var expectedWelcomeMessageSegment = IRequestProcessor.SeeValidBotCommandsInstruction.RawEnglishText;
        
        // Act
        await handler.SafelyHandleMessageAsync(startCommandMessage, botType);
        
        // Assert
        mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                startCommandMessage.Chat.Id,
                It.Is<string>(output => output.Contains(expectedWelcomeMessageSegment) && 
                                        output.Contains(botType.ToString())),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }
}
