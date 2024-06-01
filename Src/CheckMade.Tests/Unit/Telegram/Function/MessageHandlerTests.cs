using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Logic.RequestProcessors.Concrete;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.DTOs;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit.Abstractions;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace CheckMade.Tests.Unit.Telegram.Function;

public class MessageHandlerTests(ITestOutputHelper outputHelper)
{
    private ServiceProvider? _services;

    [Fact]
    // Agnostic to BotType, using Operations
    public async Task HandleMessageAsync_LogsWarningAndReturns_ForUnhandledMessageType()
    {
        var serviceCollection = new UnitTestStartup().Services;
        var mockLogger = new Mock<ILogger<MessageHandler>>();
        serviceCollection.AddScoped<ILogger<MessageHandler>>(_ => mockLogger.Object);
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        // type 'Unknown' is derived by Telegram for lack of any props!
        var unhandledMessageTypeUpdate = new UpdateWrapper(new Message { Chat = new Chat { Id = 123L } });
        var expectedLoggedMessage = $"Received message of type '{MessageType.Unknown}': " +
                                    $"{BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish()}";

        await basics.handler.HandleMessageAsync(unhandledMessageTypeUpdate, BotType.Operations);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning, 
            It.IsAny<EventId>(), 
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLoggedMessage)), 
            It.IsAny<Exception>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!));
    }
    
    [Fact]
    // Agnostic to BotType, using Operations
    public async Task HandleMessageAsync_LogsDebuggingDetails_WhenDataAccessExceptionThrown()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
                new Error(new DataAccessException("Mock DataAccess Error", new Exception()))));
        var mockLogger = new Mock<ILogger<MessageHandler>>();
        serviceCollection.AddScoped<ILogger<MessageHandler>>(_ => mockLogger.Object);
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleMessageAsync(textUpdate, BotType.Operations);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug, 
            It.IsAny<EventId>(), 
            It.IsAny<It.IsAnyType>(), 
            It.IsAny<DataAccessException>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!));
    }

    [Fact]
    public async Task HandleMessageAsync_ShowsCorrectError_ForInvalidBotCommandToOperations()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        const string invalidBotCommand = "/invalid";
        var invalidBotCommandUpdate = basics.utils.GetValidTelegramBotCommandMessage(invalidBotCommand);
        const string expectedErrorCode = "W3DL9";
    
        // Writing out to OutputHelper to see the entire error message, as an additional manual verification
        basics.mockBotClient
            .Setup(
                x => x.SendTextMessageOrThrowAsync(
                    invalidBotCommandUpdate.Message.Chat.Id,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    Option<IReplyMarkup>.None(),
                    It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, string, Option<IReplyMarkup>, CancellationToken>((_, msg, _, _, _) => 
                outputHelper.WriteLine(msg));
        
        await basics.handler.HandleMessageAsync(invalidBotCommandUpdate, BotType.Operations);
    
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                invalidBotCommandUpdate.Message.Chat.Id,
                It.IsAny<string>(),
                It.Is<string>(msg => msg.Contains(expectedErrorCode)),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Theory]
    [InlineData(BotType.Operations)]
    [InlineData(BotType.Communications)]
    [InlineData(BotType.Notifications)]
    public async Task HandleMessageAsync_ShowsCorrectWelcomeMessage_UponStartCommand(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var startCommandUpdate = basics.utils.GetValidTelegramBotCommandMessage(Start.Command);
        var expectedWelcomeMessageSegment = IRequestProcessor.SeeValidBotCommandsInstruction.RawEnglishText;
        
        await basics.handler.HandleMessageAsync(startCommandUpdate, botType);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                startCommandUpdate.Message.Chat.Id,
                It.IsAny<string>(),
                It.Is<string>(output => output.Contains(expectedWelcomeMessageSegment) && 
                                        output.Contains(botType.ToString())),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    // Agnostic to BotType, using Operations
    public async Task HandleMessageAsync_ReturnsEnglishTestString_ForEnglishLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
                OutputDto.Create(ITestUtils.EnglishUiStringForTests)));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var updateEn = basics.utils.GetValidTelegramTextMessage("random valid text");
        updateEn.Message.From!.LanguageCode = LanguageCode.en.ToString();
        
        await basics.handler.HandleMessageAsync(updateEn, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                updateEn.Message.Chat.Id,
                It.IsAny<string>(),
                ITestUtils.EnglishUiStringForTests.GetFormattedEnglish(),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Operations
    public async Task HandleMessageAsync_ReturnsGermanTestString_ForGermanLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
                OutputDto.Create(ITestUtils.EnglishUiStringForTests)));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var updateDe = basics.utils.GetValidTelegramTextMessage("random valid text");
        updateDe.Message.From!.LanguageCode = LanguageCode.de.ToString();
        
        await basics.handler.HandleMessageAsync(updateDe, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                updateDe.Message.Chat.Id,
                It.IsAny<string>(),
                ITestUtils.GermanStringForTests,
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Operations
    public async Task HandleMessageAsync_ReturnsEnglishTestString_ForUnsupportedLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
                OutputDto.Create(ITestUtils.EnglishUiStringForTests)));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var updateUnsupportedLanguage = 
            basics.utils.GetValidTelegramTextMessage("random valid text");
        updateUnsupportedLanguage.Message.From!.LanguageCode = "xyz";
        
        await basics.handler.HandleMessageAsync(updateUnsupportedLanguage, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                updateUnsupportedLanguage.Message.Chat.Id,
                It.IsAny<string>(),
                ITestUtils.EnglishUiStringForTests.GetFormattedEnglish(),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    // Agnostic to BotType, using Operations
    // Just to confirm basic integration. Detailed unit tests for correct Output->ReplyMarkup conversions are elsewhere.
    public async Task HandleMessageAsync_SendsMessageWithCorrectReplyMarkup_ForOutputWithPrompts()
    {
        var serviceCollection = new UnitTestStartup().Services;
        var fakeOutputDto = OutputDto.Create(
            ITestUtils.EnglishUiStringForTests,
            new[] { ControlPrompts.Bad, ControlPrompts.Good });
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(fakeOutputDto));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        var converter = basics.markupConverterFactory.Create(basics.emptyTranslator);
        var expectedReplyMarkup = converter.GetReplyMarkup(fakeOutputDto);
        
        var actualMarkup = Option<IReplyMarkup>.None();
        basics.mockBotClient
            .Setup(
                x => x.SendTextMessageOrThrowAsync(
                    It.IsAny<ChatId>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Option<IReplyMarkup>>(),
                    It.IsAny<CancellationToken>())
            )
            .Callback<ChatId, string, string, Option<IReplyMarkup>, CancellationToken>(
                (_, _, _, markup, _) => actualMarkup = markup
            );
        
        await basics.handler.HandleMessageAsync(textUpdate, BotType.Operations);

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
        GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
            Attempt<OutputDto> returnValue)
    {
        var mockOperationsRequestProcessor = new Mock<IOperationsRequestProcessor>();
        
        mockOperationsRequestProcessor
            .Setup<Task<Attempt<OutputDto>>>(rp => 
                rp.ProcessRequestAsync(It.IsAny<InputMessageDto>()))
            .Returns(Task.FromResult(returnValue));

        var mockRequestProcessorSelector = new Mock<IRequestProcessorSelector>();
        
        mockRequestProcessorSelector
            .Setup(rps => 
                rps.GetRequestProcessor(BotType.Operations))
            .Returns(mockOperationsRequestProcessor.Object);

        return mockRequestProcessorSelector.Object;
    }
}
