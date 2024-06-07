using CheckMade.Common.Interfaces.Persistence;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversion;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Logic.UpdateProcessors;
using CheckMade.Telegram.Logic.UpdateProcessors.Concrete;
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
    public async Task HandleUpdateAsync_LogsError_WhenUpdateProcessorThrowsException()
    {
        var serviceCollection = new UnitTestStartup().Services;
        var mockIUpdateProcessorSelector = new Mock<IUpdateProcessorSelector>();
        var mockOperationsUpdateProcessor = new Mock<IOperationsUpdateProcessor>();
        
        mockOperationsUpdateProcessor
            .Setup(opr => opr.ProcessUpdateAsync(It.IsAny<Result<TelegramUpdate>>()))
            .Throws<Exception>();
        mockIUpdateProcessorSelector
            .Setup(x => x.GetUpdateProcessor(BotType.Operations))
            .Returns(mockOperationsUpdateProcessor.Object);
        
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ => mockIUpdateProcessorSelector.Object);
        var mockLogger = new Mock<ILogger<UpdateHandler>>();
        serviceCollection.AddScoped<ILogger<UpdateHandler>>(_ => mockLogger.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(textUpdate, BotType.Operations);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Error, 
            It.IsAny<EventId>(), 
            It.IsAny<It.IsAnyType>(), 
            It.IsAny<Exception>(), 
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
                x => x.SendTextMessageAsync(
                    invalidBotCommandUpdate.Message.Chat.Id,
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    Option<IReplyMarkup>.None(),
                    It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, string, Option<IReplyMarkup>, CancellationToken>((_, msg, _, _, _) => 
                outputHelper.WriteLine(msg));
        
        await basics.handler.HandleUpdateAsync(invalidBotCommandUpdate, BotType.Operations);
    
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
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
        var expectedWelcomeMessageSegment = IUpdateProcessor.SeeValidBotCommandsInstruction.RawEnglishText;
        
        await basics.handler.HandleUpdateAsync(startCommandUpdate, botType);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
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
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ => 
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(
                new List<OutputDto>{ new() { Text = ITestUtils.EnglishUiStringForTests } }));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var updateEn = basics.utils.GetValidTelegramTextMessage("random valid text");
        updateEn.Message.From!.LanguageCode = LanguageCode.en.ToString();
        
        await basics.handler.HandleUpdateAsync(updateEn, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
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
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ => 
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(
               new List<OutputDto>{ new() { Text = ITestUtils.EnglishUiStringForTests } }));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var updateDe = basics.utils.GetValidTelegramTextMessage("random valid text");
        updateDe.Message.From!.LanguageCode = LanguageCode.de.ToString();
        
        await basics.handler.HandleUpdateAsync(updateDe, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
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
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ => 
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(
                new List<OutputDto>{ new() { Text = ITestUtils.EnglishUiStringForTests } }));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var updateUnsupportedLanguage = 
            basics.utils.GetValidTelegramTextMessage("random valid text");
        updateUnsupportedLanguage.Message.From!.LanguageCode = "xyz";
        
        await basics.handler.HandleUpdateAsync(updateUnsupportedLanguage, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
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
            new ()
            {
              ExplicitDestination = new TelegramOutputDestination(TestUtils.SanitaryOpsAdmin1, BotType.Operations),
              Text = ITestUtils.EnglishUiStringForTests,
              ControlPromptsSelection = new[] { ControlPrompts.Bad, ControlPrompts.Good } 
            }
        };
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ => 
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(fakeOutputDto));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        var converter = basics.markupConverterFactory.Create(basics.emptyTranslator);
        var expectedReplyMarkup = converter.GetReplyMarkup(fakeOutputDto[0]);
        
        var actualMarkup = Option<IReplyMarkup>.None();
        basics.mockBotClient
            .Setup(
                x => x.SendTextMessageAsync(
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
            new OutputDto { Text = UiNoTranslate("Output1") },
            new OutputDto { Text = UiNoTranslate("Output2") }
        ];
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ =>
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(fakeListOfOutputDtos));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(update, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<Option<IReplyMarkup>>(), It.IsAny<CancellationToken>()),
            Times.Exactly(fakeListOfOutputDtos.Count));
    }

    [Fact]
    public async Task HandleUpdateAsync_SendsMessagesToExplicitDestinations_WhenOutputDestinationToChatIdMappingsExist()
    {
        var serviceCollection = new UnitTestStartup().Services;
        List<OutputDto> fakeListOfOutputDtos = [
            new OutputDto
            { 
                ExplicitDestination = new TelegramOutputDestination(
                    TestUtils.SanitaryOpsInspector1, BotType.Operations), 
                Text = UiNoTranslate("Output1: Send to Inspector1 on OperationsBot - mapping exists")   
            },
            new OutputDto
            {
                ExplicitDestination = new TelegramOutputDestination(
                    TestUtils.SanitaryOpsInspector1, BotType.Communications),
                Text = UiNoTranslate("Output2: Send to Inspector1 on CommunicationsBot - mapping exists") 
            },
            new OutputDto
            {
                ExplicitDestination = new TelegramOutputDestination(
                    TestUtils.SanitaryOpsEngineer1, BotType.Notifications),
                Text = UiNoTranslate("Output3: Send to Engineer1 on NotificationsBot - mapping exists)") 
            }
        ];
        
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ =>
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(fakeListOfOutputDtos));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");

        var expectedSendParamSets = fakeListOfOutputDtos
            .Select(output => new 
            {
                Text = output.Text.GetValueOrThrow().GetFormattedEnglish(),
                DestinationChatId = basics.chatIdByOutputDestination
                    [output.ExplicitDestination.GetValueOrThrow()].Id
            });

        await basics.handler.HandleUpdateAsync(update, BotType.Operations);

        foreach (var expectedParamSet in expectedSendParamSets)
        {
            basics.mockBotClient.Verify(
                x => x.SendTextMessageAsync(
                    expectedParamSet.DestinationChatId,
                    It.IsAny<string>(),
                    expectedParamSet.Text,
                    It.IsAny<Option<IReplyMarkup>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task HandleUpdateAsync_SendsToUpdateReceivingChatId_WhenOutputDtoHasNoDestination()
    {
        var serviceCollection = new UnitTestStartup().Services;
        const string fakeOutputMessage = "Output without destination";
        const BotType actualBotType = BotType.Communications;
        
        List<OutputDto> outputWithoutDestination = [ new OutputDto{ Text = UiNoTranslate(fakeOutputMessage) } ];
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ =>
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(
                outputWithoutDestination, actualBotType));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");
        update.Message.Chat.Id = 12345654321L;
        var expectedChatId = update.Message.Chat.Id;
    
        await basics.handler.HandleUpdateAsync(update, actualBotType);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                expectedChatId,
                It.IsAny<string>(),
                fakeOutputMessage,
                It.IsAny<Option<IReplyMarkup>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        // Cannot test for correct botClient.MyBotType because the mockBotClient is not (yet) BotType-specific!
    }

    [Fact]
    public async Task HandleUpdateAsync_SendsMultipleAttachmentTypes_WhenOutputContainsThem()
    {
        var serviceCollection = new UnitTestStartup().Services;
        List<OutputDto> outputWithPhoto =
        [
            new OutputDto
            {
                ExplicitDestination = new TelegramOutputDestination(
                    TestUtils.SanitaryOpsCleanLead1, BotType.Operations),
                
                Text = UiNoTranslate("These photos' caption"),
                
                Attachments = new List<OutputAttachmentDetails>
                {
                    new(new Uri("https://www.gorin.de/fakeUri.html"), 
                        AttachmentType.Photo, Option<UiString>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri2.html"), 
                        AttachmentType.Photo, Option<UiString>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri3.html"), 
                        AttachmentType.Voice, Option<UiString>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri4.html"), 
                        AttachmentType.Document, Option<UiString>.None())
                } 
            }
        ];
        
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ =>
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(outputWithPhoto));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("Hey, send me some attachments!");

        await basics.handler.HandleUpdateAsync(update, BotType.Operations);

        basics.mockBotClient.Verify(
            x => x.SendPhotoAsync(
                It.IsAny<AttachmentSendOutParameters>(),
                It.IsAny<CancellationToken>()),
                Times.Exactly(2));

        basics.mockBotClient.Verify(
            x => x.SendVoiceAsync(
                It.IsAny<AttachmentSendOutParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
        
        basics.mockBotClient.Verify(
            x => x.SendDocumentAsync(
                It.IsAny<AttachmentSendOutParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [Fact]
    public async Task HandleUpdateAsync_SendsLocation_WhenOutputContainsOne()
    {
        var serviceCollection = new UnitTestStartup().Services;
        List<OutputDto> outputWithLocation =
        [
            new OutputDto
            {
                ExplicitDestination = new TelegramOutputDestination(
                    TestUtils.SanitaryOpsEngineer1, BotType.Operations),
                Text = UiNoTranslate("Go to this location now:"),
                Location = new Geo(35.098, -17.077, Option<float>.None()) 
            }
        ];
        
        serviceCollection.AddScoped<IUpdateProcessorSelector>(_ =>
            GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(outputWithLocation));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("hey where do I need to go?");

        await basics.handler.HandleUpdateAsync(update, BotType.Operations);
        
        basics.mockBotClient.Verify(
            x => x.SendLocationAsync(
                It.IsAny<ChatId>(),
                It.Is<Geo>(geo => geo == outputWithLocation[0].Location),
                It.IsAny<Option<IReplyMarkup>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    private static (ITestUtils utils, 
        Mock<IBotClientWrapper> mockBotClient,
        IUpdateHandler handler,
        IOutputToReplyMarkupConverterFactory markupConverterFactory,
        IUiTranslator emptyTranslator,
        IDictionary<TelegramOutputDestination, TelegramChatId> chatIdByOutputDestination)
        GetBasicTestingServices(IServiceProvider sp) => 
            (sp.GetRequiredService<ITestUtils>(), 
                sp.GetRequiredService<Mock<IBotClientWrapper>>(),
                sp.GetRequiredService<IUpdateHandler>(),
                sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>(),
                new UiTranslator(Option<IReadOnlyDictionary<string, string>>.None(), 
                    sp.GetRequiredService<ILogger<UiTranslator>>()),
                sp.GetRequiredService<IChatIdByOutputDestinationRepository>().GetAllAsync()
                    .Result
                    .ToDictionary(
                        keySelector: map => map.OutputDestination,
                        elementSelector: map => map.ChatId)
                );

    // Useful when we need to mock up what Telegram.Logic returns, e.g. to test Telegram.Function related mechanics
    private static IUpdateProcessorSelector 
        GetMockSelectorForOperationsUpdateProcessorWithSetUpReturnValue(
            IReadOnlyList<OutputDto> returnValue, BotType botType = BotType.Operations)
    {
        var mockOperationsUpdateProcessor = new Mock<IOperationsUpdateProcessor>();
        
        mockOperationsUpdateProcessor
            .Setup<Task<IReadOnlyList<OutputDto>>>(rp => 
                rp.ProcessUpdateAsync(It.IsAny<Result<TelegramUpdate>>()))
            .Returns(Task.FromResult(returnValue));

        var mockUpdateProcessorSelector = new Mock<IUpdateProcessorSelector>();
        
        mockUpdateProcessorSelector
            .Setup(rps => 
                rps.GetUpdateProcessor(botType))
            .Returns(mockOperationsUpdateProcessor.Object);

        return mockUpdateProcessorSelector.Object;
    }
}
