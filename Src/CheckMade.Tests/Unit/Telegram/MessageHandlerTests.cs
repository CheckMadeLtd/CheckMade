using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Logic.RequestProcessors.ByBotType;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.BotPrompts;
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
    public async Task HandleMessageAsync_SendsCorrectEchoMessageByBotType_ForValidTextMessage(
        BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textMessage = basics.utils.GetValidTelegramTextMessage("simple valid text");
        var expectedOutputMessage = $"Echo from bot {botType}: {textMessage.Text}";

        await basics.handler.HandleMessageAsync(textMessage, botType);
        
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
        var basics = GetBasicTestingServices(_services);
        var expectedOutputMessage = $"Echo from bot Submissions: {attachmentType}";
        
        var attachmentMessage = attachmentType switch
        {
            AttachmentType.Audio => basics.utils.GetValidTelegramAudioMessage(),
            AttachmentType.Document => basics.utils.GetValidTelegramDocumentMessage(),
            AttachmentType.Photo => basics.utils.GetValidTelegramPhotoMessage(),
            AttachmentType.Video => basics.utils.GetValidTelegramVideoMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        await basics.handler.HandleMessageAsync(attachmentMessage, BotType.Submissions);
        
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
        var mockLogger = new Mock<ILogger<MessageHandler>>();
        serviceCollection.AddScoped<ILogger<MessageHandler>>(_ => mockLogger.Object);
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        // type 'Unknown' is derived by Telegram for lack of any props!
        var unknownMessage = new Message { Chat = new Chat { Id = 123L } };
        var expectedLoggedMessage = $"Received message of type '{MessageType.Unknown}': " +
                                    $"{BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish()}";

        await basics.handler.HandleMessageAsync(unknownMessage, BotType.Submissions);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning, 
            It.IsAny<EventId>(), 
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLoggedMessage)), 
            It.IsAny<Exception>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!));
    }
    
    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task HandleMessageAsync_OutputsCorrectErrorMessage_WhenDataAccessExceptionThrown()
    {
        var serviceCollection = new UnitTestStartup().Services;
        const string mockErrorMessage = "Mock DataAccess Error";
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                new Failure(new DataAccessException(mockErrorMessage, new Exception()))));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textMessage = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleMessageAsync(textMessage, BotType.Submissions);
        
        basics.mockBotClient.Verify(x => x.SendTextMessageOrThrowAsync(
            It.IsAny<ChatId>(), 
            It.Is<string>(output => output.Contains(mockErrorMessage)),
            Option<IReplyMarkup>.None(),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task HandleMessageAsync_ShowsCorrectError_ForInvalidBotCommandToSubmissions()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string invalidBotCommand = "/invalid";
        var invalidBotCommandMessage = basics.utils.GetValidTelegramBotCommandMessage(invalidBotCommand);
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
        
        await basics.handler.HandleMessageAsync(invalidBotCommandMessage, BotType.Submissions);
    
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
        var basics = GetBasicTestingServices(_services);
        var startCommandMessage = basics.utils.GetValidTelegramBotCommandMessage(Start.Command);
        var expectedWelcomeMessageSegment = IRequestProcessor.SeeValidBotCommandsInstruction.RawEnglishText;
        
        await basics.handler.HandleMessageAsync(startCommandMessage, botType);
        
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
    public async Task HandleMessageAsync_ReturnsEnglishTestString_ForEnglishLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                new OutputDto(
                    ITestUtils.EnglishUiStringForTests,
                    Option<IEnumerable<EBotPrompts>>.None(), 
                    Option<IEnumerable<string>>.None())));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var messageWithEnglishInTlgrUserLangSetting = basics.utils.GetValidTelegramTextMessage("random valid text");
        messageWithEnglishInTlgrUserLangSetting.From!.LanguageCode = LanguageCode.en.ToString();
        
        await basics.handler.HandleMessageAsync(messageWithEnglishInTlgrUserLangSetting, BotType.Submissions);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                messageWithEnglishInTlgrUserLangSetting.Chat.Id,
                ITestUtils.EnglishUiStringForTests.GetFormattedEnglish(),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task HandleMessageAsync_ReturnsGermanTestString_ForGermanLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                new OutputDto(
                    ITestUtils.EnglishUiStringForTests,
                    Option<IEnumerable<EBotPrompts>>.None(), 
                    Option<IEnumerable<string>>.None())));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var messageWithGermanInTlgrUserLangSetting = basics.utils.GetValidTelegramTextMessage("random valid text");
        messageWithGermanInTlgrUserLangSetting.From!.LanguageCode = LanguageCode.de.ToString();
        
        await basics.handler.HandleMessageAsync(messageWithGermanInTlgrUserLangSetting, BotType.Submissions);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                messageWithGermanInTlgrUserLangSetting.Chat.Id,
                ITestUtils.GermanStringForTests,
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Submissions
    public async Task HandleMessageAsync_ReturnsEnglishTestString_ForUnsupportedLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
                new OutputDto(
                    ITestUtils.EnglishUiStringForTests,
                    Option<IEnumerable<EBotPrompts>>.None(), 
                    Option<IEnumerable<string>>.None())));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var msgWithUnsupportedLangInTlgrUserSetting = 
            basics.utils.GetValidTelegramTextMessage("random valid text");
        msgWithUnsupportedLangInTlgrUserSetting.From!.LanguageCode = "xyz";
        
        await basics.handler.HandleMessageAsync(msgWithUnsupportedLangInTlgrUserSetting, BotType.Submissions);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                msgWithUnsupportedLangInTlgrUserSetting.Chat.Id,
                ITestUtils.EnglishUiStringForTests.GetFormattedEnglish(),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Submissions
    // Just to confirm basic integration. Detailed unit tests for correct Output->ReplyMarkup conversions are elsewhere.
    public async Task HandleMessageAsync_SendsMessageWithCorrectReplyMarkup_ForOutputWithPrompts()
    {
        var serviceCollection = new UnitTestStartup().Services;
        var fakeOutputDto = new OutputDto(
            ITestUtils.EnglishUiStringForTests,
            new[] { EBotPrompts.Bad, EBotPrompts.Good },
            Option<IEnumerable<string>>.None());
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(fakeOutputDto));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textMessage = basics.utils.GetValidTelegramTextMessage("random valid text");
        var converter = basics.markupConverterFactory.Create(basics.emptyTranslator);
        var expectedReplyMarkup = converter.GetReplyMarkup(fakeOutputDto);
        
        var actualMarkup = Option<IReplyMarkup>.None();
        basics.mockBotClient
            .Setup(
                x => x.SendTextMessageOrThrowAsync(
                    It.IsAny<ChatId>(),
                    It.IsAny<string>(),
                    It.IsAny<Option<IReplyMarkup>>(),
                    It.IsAny<CancellationToken>())
            )
            .Callback<ChatId, string, Option<IReplyMarkup>, CancellationToken>(
                (_, _, markup, _) => actualMarkup = markup
            );
        
        await basics.handler.HandleMessageAsync(textMessage, BotType.Submissions);

        Assert.Equivalent(expectedReplyMarkup, actualMarkup);
    }
    
    private static (ITestUtils utils, Mock<IBotClientWrapper> mockBotClient, IMessageHandler handler,
        IOutputToReplyMarkupConverterFactory markupConverterFactory, IUiTranslator emptyTranslator)
        GetBasicTestingServices(IServiceProvider sp) => 
        (sp.GetRequiredService<ITestUtils>(), 
            sp.GetRequiredService<Mock<IBotClientWrapper>>(),
            sp.GetRequiredService<IMessageHandler>(),
            sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>(),
            new UiTranslator(Option<IReadOnlyDictionary<string, string>>.None(), 
                sp.GetRequiredService<ILogger<UiTranslator>>()));

    // Useful when we need to mock up what Telegram.Logic returns, e.g. to test Telegram.Function related mechanics
    private static IRequestProcessorSelector 
        GetMockSelectorForSubmissionsRequestProcessorWithSetUpReturnValue(
            Attempt<OutputDto> returnValue)
    {
        var mockSubmissionsRequestProcessor = new Mock<ISubmissionsRequestProcessor>();
        
        mockSubmissionsRequestProcessor
            .Setup<Task<Attempt<OutputDto>>>(rp => 
                rp.ProcessRequestAsync(It.IsAny<InputMessageDto>()))
            .Returns(Task.FromResult(returnValue));

        var mockRequestProcessorSelector = new Mock<IRequestProcessorSelector>();
        
        mockRequestProcessorSelector
            .Setup(rps => 
                rps.GetRequestProcessor(BotType.Submissions))
            .Returns(mockSubmissionsRequestProcessor.Object);

        return mockRequestProcessorSelector.Object;
    }
}
