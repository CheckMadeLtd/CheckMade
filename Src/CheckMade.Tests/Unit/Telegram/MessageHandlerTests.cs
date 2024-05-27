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
using Telegram.Bot.Types.ReplyMarkups;
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
        var basics = GetBasicTestingServices(_services);
        
        var textMessage = basics.utils.GetValidTelegramTextMessage("simple valid text");
        var expectedOutputMessage = $"Echo from bot {botType}: {textMessage.Text}";

        // Act
        await basics.handler.SafelyHandleMessageAsync(textMessage, botType);
        
        // Assert
        basics.mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
                textMessage.Chat.Id,
                expectedOutputMessage,
                Option<IReplyMarkup>.None(),
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
        var basics = GetBasicTestingServices(_services);
        
        var attachmentMessage = attachmentType switch
        {
            AttachmentType.Audio => basics.utils.GetValidTelegramAudioMessage(),
            AttachmentType.Document => basics.utils.GetValidTelegramDocumentMessage(),
            AttachmentType.Photo => basics.utils.GetValidTelegramPhotoMessage(),
            AttachmentType.Video => basics.utils.GetValidTelegramVideoMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var expectedOutputMessage = $"Echo from bot Submissions: {attachmentType}";
        
        // Act
        await basics.handler.SafelyHandleMessageAsync(attachmentMessage, BotType.Submissions);
        
        // Assert
        basics.mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
                attachmentMessage.Chat.Id,
                expectedOutputMessage, 
                Option<IReplyMarkup>.None(),
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
        
        var basics = GetBasicTestingServices(_services);
        
        // Act 
        await basics.handler.SafelyHandleMessageAsync(unknownMessage, BotType.Submissions);
        
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

        var basics = GetBasicTestingServices(_services);
        
        basics.mockBotClient
            .Setup(x => x.SendTextMessageOrThrowAsync(
                It.IsAny<ChatId>(), 
                It.Is<string>(output => output.Contains(mockErrorMessage)),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()))
            .Verifiable();

        var textMessage = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        // Act 
        await basics.handler.SafelyHandleMessageAsync(textMessage, BotType.Submissions);
        
        // Assert
        basics.mockBotClient.Verify();
    }

    [Fact]
    public async Task HandleMessageAsync_EchosCorrectBotCommandCode_ForValidBotCommandInputToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var basics = GetBasicTestingServices(_services);
        
        var validBotCommand = new BotCommandMenus()
            .SubmissionsBotCommandMenu[SubmissionsBotCommands.Problem][0].Command;
        var botCommandMessage = basics.utils.GetBotCommandMessage(validBotCommand);
        var expectedOutputMessage = $"Echo of a Submissions BotCommand: {(int)SubmissionsBotCommands.Problem}";

        // Act
        await basics.handler.SafelyHandleMessageAsync(botCommandMessage, BotType.Submissions);

        // Assert
        basics.mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
                botCommandMessage.Chat.Id,
                expectedOutputMessage,
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task HandleMessageAsync_ShowsCorrectError_ForInvalidBotCommandToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var basics = GetBasicTestingServices(_services);
        
        const string invalidBotCommand = "/invalid";
        var invalidBotCommandMessage = basics.utils.GetBotCommandMessage(invalidBotCommand);
        const string expectedErrorCode = "W3DL9";
    
        basics.mockBotClient
            .Setup(
                x => x.SendTextMessageOrThrowAsync(
                    invalidBotCommandMessage.Chat.Id,
                    It.IsAny<string>(),
                    Option<IReplyMarkup>.None(),
                    It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, Option<IReplyMarkup>, CancellationToken>((_, msg, _, _) => 
                outputHelper.WriteLine(msg));
        
        // Act
        await basics.handler.SafelyHandleMessageAsync(invalidBotCommandMessage, BotType.Submissions);
    
        // Assert
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                invalidBotCommandMessage.Chat.Id,
                It.Is<string>(msg => msg.Contains(expectedErrorCode)),
                Option<IReplyMarkup>.None(),
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
        var basics = GetBasicTestingServices(_services);
        
        var startCommandMessage = basics.utils.GetBotCommandMessage(Start.Command);
        var expectedWelcomeMessageSegment = IRequestProcessor.SeeValidBotCommandsInstruction.RawEnglishText;
        
        // Act
        await basics.handler.SafelyHandleMessageAsync(startCommandMessage, botType);
        
        // Assert
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                startCommandMessage.Chat.Id,
                It.Is<string>(output => output.Contains(expectedWelcomeMessageSegment) && 
                                        output.Contains(botType.ToString())),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    // [Fact]
    // public void SafelyHandleMessageAsync_ReturnsEnglishTestString_ForUnsupportedLanguageCode()
    // {
    //     _services = new UnitTestStartup().Services.BuildServiceProvider();
    //     
    //     // Arrange
    //     var fakeInvalidLanguageCodeInTelegramMessage = "xyz";
    //     var basics = GetBasicTestingServices(_services);
    //     
    //     // Act
    //     
    // }

    private (ITestUtils utils, Mock<IBotClientWrapper> mockBotClient, IMessageHandler handler)
        GetBasicTestingServices(IServiceProvider services) => 
        (services.GetRequiredService<ITestUtils>(), 
            services.GetRequiredService<Mock<IBotClientWrapper>>(),
            services.GetRequiredService<IMessageHandler>());
}
