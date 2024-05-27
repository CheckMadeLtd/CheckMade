using CheckMade.Common.LangExt;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.BotCommands.DefinitionEnumsByBotType;
using CheckMade.Telegram.Model.DTOs;
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
    public async Task SafelyHandleMessageAsync_SendsCorrectEchoMessageByBotType_ForValidTextMessage(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        // Arrange
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
    public async Task SafelyHandleMessageAsync_SendsCorrectEchoMessage_ForValidAttachmentMessageToSubmissions(
        AttachmentType attachmentType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        // Arrange
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
    public async Task SafelyHandleMessageAsync_LogsWarningAndReturns_ForUnhandledMessageType()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        var mockLogger = new Mock<ILogger<MessageHandler>>();
        serviceCollection.AddScoped<ILogger<MessageHandler>>(_ => mockLogger.Object);
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var unknownMessage = new Message // type 'Unknown' is derived by Telegram for lack of any props!
        {
            Chat = new Chat { Id = 123L }
        };
        
        var expectedLoggedMessage = $"Received message of type '{MessageType.Unknown}': " +
                                    $"{BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish()}";

        // Act 
        await basics.handler.SafelyHandleMessageAsync(unknownMessage, BotType.Submissions);
        
        // Assert
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning, 
            It.IsAny<EventId>(), 
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLoggedMessage)), 
            It.IsAny<Exception>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!));
    }
    
    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task SafelyHandleMessageAsync_OutputsCorrectErrorMessage_WhenDataAccessExceptionThrown()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        const string mockErrorMessage = "Mock DataAccess Error";
        
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                new Failure(new DataAccessException(mockErrorMessage, new Exception()))));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var textMessage = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        // Act 
        await basics.handler.SafelyHandleMessageAsync(textMessage, BotType.Submissions);
        
        // Assert
        basics.mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
            It.IsAny<ChatId>(), 
            It.Is<string>(output => output.Contains(mockErrorMessage)),
            Option<IReplyMarkup>.None(),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task SafelyHandleMessageAsync_EchosCorrectBotCommandCode_ForValidBotCommandInputToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        // Arrange
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
    public async Task SafelyHandleMessageAsync_ShowsCorrectError_ForInvalidBotCommandToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        // Arrange
        const string invalidBotCommand = "/invalid";
        var invalidBotCommandMessage = basics.utils.GetBotCommandMessage(invalidBotCommand);
        const string expectedErrorCode = "W3DL9";
    
        // Writing out to OutputHelper to see the entire error message, as an additional manual verification
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
    public async Task SafelyHandleMessageAsync_ShowsCorrectWelcomeMessage_UponStartCommand(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        // Arrange
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

    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task SafelyHandleMessageAsync_ReturnsEnglishTestString_ForEnglishLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                ITestUtils.EnglishUiStringForTests));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var messageWithEnglishInTlgrUserLangSetting = basics.utils.GetValidTelegramTextMessage("random valid text");
        messageWithEnglishInTlgrUserLangSetting.From!.LanguageCode = LanguageCode.en.ToString();
        
        // Act
        await basics.handler.SafelyHandleMessageAsync(messageWithEnglishInTlgrUserLangSetting, BotType.Submissions);
        
        // Assert
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                messageWithEnglishInTlgrUserLangSetting.Chat.Id,
                ITestUtils.EnglishUiStringForTests.GetFormattedEnglish(),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task SafelyHandleMessageAsync_ReturnsGermanTestString_ForGermanLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                ITestUtils.EnglishUiStringForTests));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var messageWithGermanInTlgrUserLangSetting = basics.utils.GetValidTelegramTextMessage("random valid text");
        messageWithGermanInTlgrUserLangSetting.From!.LanguageCode = LanguageCode.de.ToString();
        
        // Act
        await basics.handler.SafelyHandleMessageAsync(messageWithGermanInTlgrUserLangSetting, BotType.Submissions);
        
        // Assert
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                messageWithGermanInTlgrUserLangSetting.Chat.Id,
                ITestUtils.GermanStringForTests,
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task SafelyHandleMessageAsync_ReturnsEnglishTestString_ForUnsupportedLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        // Arrange
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                ITestUtils.EnglishUiStringForTests));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var msgWithUnsupportedLangInTlgrUserSetting = 
            basics.utils.GetValidTelegramTextMessage("random valid text");
        msgWithUnsupportedLangInTlgrUserSetting.From!.LanguageCode = "xyz";
        
        // Act
        await basics.handler.SafelyHandleMessageAsync(msgWithUnsupportedLangInTlgrUserSetting, BotType.Submissions);
        
        // Assert
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                msgWithUnsupportedLangInTlgrUserSetting.Chat.Id,
                ITestUtils.EnglishUiStringForTests.GetFormattedEnglish(),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    private static (ITestUtils utils, Mock<IBotClientWrapper> mockBotClient, IMessageHandler handler)
        GetBasicTestingServices(IServiceProvider sp) => 
        (sp.GetRequiredService<ITestUtils>(), 
            sp.GetRequiredService<Mock<IBotClientWrapper>>(),
            sp.GetRequiredService<IMessageHandler>());

    // Useful when we need to mock up what Telegram.Logic returns, e.g. to test Telegram.Function related mechanics
    private static IRequestProcessorSelector 
        GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(Attempt<UiString> returnValue)
    {
        var mockSubmissionsRequestProcessor = new Mock<ISubmissionsRequestProcessor>();
        
        mockSubmissionsRequestProcessor
            .Setup<Task<Attempt<UiString>>>(rp => 
                rp.SafelyEchoAsync(It.IsAny<InputMessageDto>()))
            .Returns(Task.FromResult(returnValue));

        var mockRequestProcessorSelector = new Mock<IRequestProcessorSelector>();
        
        mockRequestProcessorSelector
            .Setup(rps => 
                rps.GetRequestProcessor(BotType.Submissions))
            .Returns(mockSubmissionsRequestProcessor.Object);

        return mockRequestProcessorSelector.Object;
    }
}
