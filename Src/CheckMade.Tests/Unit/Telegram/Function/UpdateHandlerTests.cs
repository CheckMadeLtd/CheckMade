using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Logic.RequestProcessors.Concrete;
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

public class UpdateHandlerTests(ITestOutputHelper outputHelper)
{
    private ServiceProvider? _services;

    [Fact]
    // Agnostic to BotType, using Operations
    public async Task HandleUpdateAsync_LogsWarningAndReturns_ForUnhandledMessageType()
    {
        var serviceCollection = new UnitTestStartup().Services;
        var mockLogger = new Mock<ILogger<UpdateHandler>>();
        serviceCollection.AddScoped<ILogger<UpdateHandler>>(_ => mockLogger.Object);
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        // type 'Unknown' is derived by Telegram for lack of any props!
        var unhandledMessageTypeUpdate = new UpdateWrapper(new Message { Chat = new Chat { Id = 123L } });
        var expectedLoggedMessage = $"Received message of type '{MessageType.Unknown}': " +
                                    $"{BotUpdateSwitch.NoSpecialHandlingWarning.GetFormattedEnglish()}";

        await basics.handler.HandleUpdateAsync(unhandledMessageTypeUpdate, BotType.Operations);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning, 
            It.IsAny<EventId>(), 
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLoggedMessage)), 
            It.IsAny<Exception>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!));
    }
    
    [Fact]
    // Agnostic to BotType, using Operations
    public async Task HandleUpdateAsync_LogsDebuggingDetails_WhenDataAccessExceptionThrown()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
                new Error(new DataAccessException("Mock DataAccess Error", new Exception()))));
        var mockLogger = new Mock<ILogger<UpdateHandler>>();
        serviceCollection.AddScoped<ILogger<UpdateHandler>>(_ => mockLogger.Object);
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(textUpdate, BotType.Operations);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug, 
            It.IsAny<EventId>(), 
            It.IsAny<It.IsAnyType>(), 
            It.IsAny<DataAccessException>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!));
    }

    [Fact]
    public async Task HandleUpdateAsync_ShowsCorrectError_ForInvalidBotCommandToOperations()
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
        
        await basics.handler.HandleUpdateAsync(invalidBotCommandUpdate, BotType.Operations);
    
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
    public async Task HandleUpdateAsync_ShowsCorrectWelcomeMessage_UponStartCommand(BotType botType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var startCommandUpdate = basics.utils.GetValidTelegramBotCommandMessage(Start.Command);
        var expectedWelcomeMessageSegment = IRequestProcessor.SeeValidBotCommandsInstruction.RawEnglishText;
        
        await basics.handler.HandleUpdateAsync(startCommandUpdate, botType);
        
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
    public async Task HandleUpdateAsync_ReturnsEnglishTestString_ForEnglishLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
                new List<OutputDto>{ OutputDto.Create(ITestUtils.EnglishUiStringForTests) }));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var updateEn = basics.utils.GetValidTelegramTextMessage("random valid text");
        updateEn.Message.From!.LanguageCode = LanguageCode.en.ToString();
        
        await basics.handler.HandleUpdateAsync(updateEn, BotType.Operations);
        
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
    public async Task HandleUpdateAsync_ReturnsGermanTestString_ForGermanLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
               new List<OutputDto>{ OutputDto.Create(ITestUtils.EnglishUiStringForTests) }));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var updateDe = basics.utils.GetValidTelegramTextMessage("random valid text");
        updateDe.Message.From!.LanguageCode = LanguageCode.de.ToString();
        
        await basics.handler.HandleUpdateAsync(updateDe, BotType.Operations);
        
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
    public async Task HandleUpdateAsync_ReturnsEnglishTestString_ForUnsupportedLanguageCode()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
                new List<OutputDto>{ OutputDto.Create(ITestUtils.EnglishUiStringForTests) }));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var updateUnsupportedLanguage = 
            basics.utils.GetValidTelegramTextMessage("random valid text");
        updateUnsupportedLanguage.Message.From!.LanguageCode = "xyz";
        
        await basics.handler.HandleUpdateAsync(updateUnsupportedLanguage, BotType.Operations);
        
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
    public async Task HandleUpdateAsync_SendsMessageWithCorrectReplyMarkup_ForOutputWithPrompts()
    {
        var serviceCollection = new UnitTestStartup().Services;
        var fakeOutputDto = new List<OutputDto>{ 
            OutputDto.Create(
                new OutputDestination(BotType.Operations, TestUtils.SanitaryOpsAdmin1),
                ITestUtils.EnglishUiStringForTests, 
                new[] { ControlPrompts.Bad, ControlPrompts.Good }) 
        };
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ => 
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(fakeOutputDto));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        var converter = basics.markupConverterFactory.Create(basics.emptyTranslator);
        var expectedReplyMarkup = converter.GetReplyMarkup(fakeOutputDto[0]);
        
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
        
        await basics.handler.HandleUpdateAsync(textUpdate, BotType.Operations);

        Assert.Equivalent(expectedReplyMarkup, actualMarkup);
    }

    [Fact]
    public async Task HandleUpdateAsync_SendsMultipleMessages_ForListOfOutputDtos()
    {
        var serviceCollection = new UnitTestStartup().Services;
        List<OutputDto> fakeListOfOutputDtos = [ 
            OutputDto.Create(UiNoTranslate("Output1")),
            OutputDto.Create(UiNoTranslate("Output2"))
        ];
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ =>
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(fakeListOfOutputDtos));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(update, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageOrThrowAsync(
                It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Option<IReplyMarkup>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(fakeListOfOutputDtos.Count));
    }

    [Fact]
    public async Task HandleUpdateAsync_SendsMessagesToExplicitDestinations_WhenRoleBotTypeToChatIdMappingsExist()
    {
        var serviceCollection = new UnitTestStartup().Services;
        List<OutputDto> fakeListOfOutputDtos = [
            OutputDto.Create(
                new OutputDestination(BotType.Operations, TestUtils.SanitaryOpsInspector1), 
                UiNoTranslate("Output1: Send to Inspector1 on OperationsBot - mapping exists")),
            OutputDto.Create(
                new OutputDestination(BotType.Communications, TestUtils.SanitaryOpsInspector1), 
                UiNoTranslate("Output2: Send to Inspector1 on CommunicationsBot - mapping exists")),
            OutputDto.Create(
                new OutputDestination(BotType.Notifications, TestUtils.SanitaryOpsEngineer1), 
                UiNoTranslate("Output3: Send to Engineer1 on NotificationsBot - mapping exists)"))
        ];
        serviceCollection.AddScoped<IRequestProcessorSelector>(_ =>
            GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(fakeListOfOutputDtos));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");

        var expectedSendParamSets = fakeListOfOutputDtos
            .Select(output => new 
            {
                Text = output.Text.GetValueOrDefault().GetFormattedEnglish(),
                DestinationChatId = basics.chatIdByOutputDestination
                    [output.ExplicitDestination.GetValueOrDefault()].Id
            });

        await basics.handler.HandleUpdateAsync(update, BotType.Operations);

        foreach (var expectedParamSet in expectedSendParamSets)
        {
            basics.mockBotClient.Verify(
                x => x.SendTextMessageOrThrowAsync(
                    expectedParamSet.DestinationChatId,
                    It.IsAny<string>(),
                    expectedParamSet.Text,
                    It.IsAny<Option<IReplyMarkup>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
    
    // ToDo: Add test to check correct handling of lack of explicit OutputDestination and failure to resolve ChatId
    
    private static (ITestUtils utils, 
        Mock<IBotClientWrapper> mockBotClient,
        IUpdateHandler handler,
        IOutputToReplyMarkupConverterFactory markupConverterFactory,
        IUiTranslator emptyTranslator,
        Dictionary<OutputDestination, TelegramChatId> chatIdByOutputDestination)
        GetBasicTestingServices(IServiceProvider sp) => 
            (sp.GetRequiredService<ITestUtils>(), 
                sp.GetRequiredService<Mock<IBotClientWrapper>>(),
                sp.GetRequiredService<IUpdateHandler>(),
                sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>(),
                new UiTranslator(Option<IReadOnlyDictionary<string, string>>.None(), 
                    sp.GetRequiredService<ILogger<UiTranslator>>()),
                sp.GetRequiredService<IChatIdByOutputDestinationRepository>().GetAllOrThrowAsync()
                    .Result
                    .ToDictionary(
                        keySelector: map => new OutputDestination(map.BotType, map.Role),
                        elementSelector: map => map.ChatId)
                );

    // Useful when we need to mock up what Telegram.Logic returns, e.g. to test Telegram.Function related mechanics
    private static IRequestProcessorSelector 
        GetMockSelectorForOperationsRequestProcessorWithSetUpReturnValue(
            Attempt<IReadOnlyList<OutputDto>> returnValue)
    {
        var mockOperationsRequestProcessor = new Mock<IOperationsRequestProcessor>();
        
        mockOperationsRequestProcessor
            .Setup<Task<Attempt<IReadOnlyList<OutputDto>>>>(rp => 
                rp.ProcessRequestAsync(It.IsAny<TelegramUpdate>()))
            .Returns(Task.FromResult(returnValue));

        var mockRequestProcessorSelector = new Mock<IRequestProcessorSelector>();
        
        mockRequestProcessorSelector
            .Setup(rps => 
                rps.GetRequestProcessor(BotType.Operations))
            .Returns(mockOperationsRequestProcessor.Object);

        return mockRequestProcessorSelector.Object;
    }
}
