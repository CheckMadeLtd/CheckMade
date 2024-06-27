using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit.Abstractions;
using MessageType = Telegram.Bot.Types.Enums.MessageType;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Function;

public class UpdateHandlerTests(ITestOutputHelper outputHelper)
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_LogsWarningAndReturns_ForUnhandledMessageType(InteractionMode mode)
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

        await basics.handler.HandleUpdateAsync(unhandledMessageTypeUpdate, mode);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Warning, 
            It.IsAny<EventId>(), 
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedLoggedMessage)), 
            It.IsAny<Exception>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
    }
    
    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_LogsError_WhenInputProcessorThrowsException(InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        var mockInputProcessorFactory = new Mock<IInputProcessorFactory>();
        var mockOperationsInputProcessor = new Mock<IInputProcessor>();
        
        mockOperationsInputProcessor
            .Setup(opr => opr.ProcessInputAsync(It.IsAny<Result<TlgInput>>()))
            .Throws<Exception>();
        mockInputProcessorFactory
            .Setup(x => x.GetInputProcessor(mode))
            .Returns(mockOperationsInputProcessor.Object);
        
        serviceCollection.AddScoped<IInputProcessorFactory>(_ => mockInputProcessorFactory.Object);
        var mockLogger = new Mock<ILogger<UpdateHandler>>();
        serviceCollection.AddScoped<ILogger<UpdateHandler>>(_ => mockLogger.Object);
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(textUpdate, mode);
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Error, 
            It.IsAny<EventId>(), 
            It.IsAny<It.IsAnyType>(), 
            It.IsAny<Exception>(), 
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
            Times.Once);
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
        
        await basics.handler.HandleUpdateAsync(invalidBotCommandUpdate, Operations);
    
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                invalidBotCommandUpdate.Message.Chat.Id,
                It.IsAny<string>(),
                It.Is<string>(msg => msg.Contains(expectedErrorCode)),
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_ReturnsEnglishTestString_ForEnglishSpeakingUser()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IInputProcessorFactory>(_ => 
            GetMockInputProcessorFactoryWithSetUpReturnValue(
                new List<OutputDto>{ new() { Text = EnglishUiStringForTests } }));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var updateFromEnglishUser = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(
            updateFromEnglishUser,
            TlgAgent_PrivateChat_Default.Mode);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                updateFromEnglishUser.Message.Chat.Id,
                It.IsAny<string>(),
                EnglishUiStringForTests.GetFormattedEnglish(), // untranslated English message expected
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task HandleUpdateAsync_ReturnsGermanTestString_ForGermanSpeakingUser()
    {
        var serviceCollection = new UnitTestStartup().Services;
        serviceCollection.AddScoped<IInputProcessorFactory>(_ => 
            GetMockInputProcessorFactoryWithSetUpReturnValue(
               new List<OutputDto>{ new() { Text = EnglishUiStringForTests } }));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var updateFromGermanUser = basics.utils.GetValidTelegramTextMessage(
            "random valid text",
            TlgAgent_Of_SanitaryOpsCleanLead1_ChatGroup_German.UserId,
            TlgAgent_Of_SanitaryOpsCleanLead1_ChatGroup_German.ChatId);
        
        await basics.handler.HandleUpdateAsync(
            updateFromGermanUser,
            TlgAgent_Of_SanitaryOpsCleanLead1_ChatGroup_German.Mode);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                updateFromGermanUser.Message.Chat.Id,
                It.IsAny<string>(),
                GermanStringForTests, // German translation expected
                Option<IReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }
    
    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    // Just to confirm basic integration. Detailed unit tests for correct Output->ReplyMarkup conversions are elsewhere.
    public async Task HandleUpdateAsync_SendsMessageWithCorrectReplyMarkup_ForOutputWithPrompts(InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        var outputWithPrompts = new List<OutputDto>{ 
            new ()
            {
              Text = EnglishUiStringForTests,
              ControlPromptsSelection = ControlPrompts.Bad | ControlPrompts.Good 
            }
        };
        
        serviceCollection.AddScoped<IInputProcessorFactory>(_ => 
            GetMockInputProcessorFactoryWithSetUpReturnValue(outputWithPrompts, mode));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.utils.GetValidTelegramTextMessage("random valid text");
        var converter = basics.markupConverterFactory.Create(basics.emptyTranslator);
        var expectedReplyMarkup = converter.GetReplyMarkup(outputWithPrompts[0]);
        
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
        
        await basics.handler.HandleUpdateAsync(textUpdate, mode);

        Assert.Equivalent(
            expectedReplyMarkup, 
            actualMarkup);
    }

    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_SendsMultipleMessages_ForListOfOutputDtos(InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        List<OutputDto> outputsMultiple = [ 
            new OutputDto { Text = UiNoTranslate("Output1") },
            new OutputDto { Text = UiNoTranslate("Output2") }
        ];
        
        serviceCollection.AddScoped<IInputProcessorFactory>(_ =>
            GetMockInputProcessorFactoryWithSetUpReturnValue(outputsMultiple, mode));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(update, mode);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<Option<IReplyMarkup>>(), 
                It.IsAny<CancellationToken>()),
            Times.Exactly(outputsMultiple.Count));
    }

    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_SendsMessagesToSpecifiedLogicalPorts_WhenTlgAgentRoleBindingsExist(
        InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        List<OutputDto> outputsWithLogicalPort = [
            new OutputDto
            { 
                LogicalPort = new LogicalPort(
                    SanitaryOpsInspector1_HasRoleBindings_ForAllModes, 
                    Operations), 
                Text = UiNoTranslate("Output1: Send to Inspector1 on OperationsBot")   
            },
            new OutputDto
            {
                LogicalPort = new LogicalPort(
                    SanitaryOpsInspector1_HasRoleBindings_ForAllModes, 
                    Communications),
                Text = UiNoTranslate("Output2: Send to Inspector1 on CommunicationsBot") 
            },
            new OutputDto
            {
                LogicalPort = new LogicalPort(
                    SanitaryOpsInspector1_HasRoleBindings_ForAllModes, 
                    Notifications),
                Text = UiNoTranslate("Output3: Send to Inspector1 on NotificationsBot)") 
            }
        ];
        
        serviceCollection.AddScoped<IInputProcessorFactory>(_ =>
            GetMockInputProcessorFactoryWithSetUpReturnValue(outputsWithLogicalPort, mode));
        _services = serviceCollection.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");
        var activeRoleBindings = await basics.tlgAgentRoleBindingsTask;
        
        var expectedSendParamSets = outputsWithLogicalPort
            .Select(output => new 
            {
                Text = output.Text.GetValueOrThrow().GetFormattedEnglish(),
                
                TlgChatId = activeRoleBindings
                    .First(arb => 
                        arb.Role == output.LogicalPort.GetValueOrThrow().Role &&
                        arb.TlgAgent.Mode == output.LogicalPort.GetValueOrThrow().InteractionMode)
                    .TlgAgent.ChatId.Id
            }).ToList();

        // Just asserting the internal consistency of our TestData / TestUtils setup
        Assert.Equal(
            expectedSendParamSets[0].TlgChatId,
            RoleBindFor_SanitaryOpsInspector1_InPrivateChat_OperationsMode.TlgAgent.ChatId.Id);
        Assert.Equal(
            expectedSendParamSets[0].TlgChatId,
            RoleBindFor_SanitaryOpsInspector1_InPrivateChat_CommunicationsMode.TlgAgent.ChatId.Id);
        Assert.Equal(
            expectedSendParamSets[0].TlgChatId,
            RoleBindFor_SanitaryOpsInspector1_InPrivateChat_NotificationsMode.TlgAgent.ChatId.Id);
        
        await basics.handler.HandleUpdateAsync(update, mode);

        foreach (var expectedParamSet in expectedSendParamSets)
        {
            basics.mockBotClient.Verify(
                x => x.SendTextMessageAsync(
                    expectedParamSet.TlgChatId,
                    It.IsAny<string>(),
                    expectedParamSet.Text,
                    It.IsAny<Option<IReplyMarkup>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_SendsToCurrentlyReceivingChatId_WhenOutputDtoHasNoLogicalPort(
        InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        const string fakeOutputMessage = "Output without logical port";
        List<OutputDto> outputWithoutPort = [ new OutputDto{ Text = UiNoTranslate(fakeOutputMessage) } ];
        serviceCollection.AddScoped<IInputProcessorFactory>(_ =>
            GetMockInputProcessorFactoryWithSetUpReturnValue(outputWithoutPort, mode));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        const long expectedChatId = TestChatId04;
        var update = basics.utils.GetValidTelegramTextMessage(
            "random valid text",
            TestUserId02,
            expectedChatId);
    
        await basics.handler.HandleUpdateAsync(update, mode);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                expectedChatId,
                It.IsAny<string>(),
                fakeOutputMessage,
                It.IsAny<Option<IReplyMarkup>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        // Cannot test for correct botClient.MyInteractionMode because mockBotClient is not (yet) Mode-specific!
    }

    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_SendsMultipleAttachmentTypes_WhenOutputContainsThem(InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        List<OutputDto> outputWithMultipleAttachmentTypes =
        [
            new OutputDto
            {
                Attachments = new List<OutputAttachmentDetails>
                {
                    new(new Uri("https://www.gorin.de/fakeUri1.html"), 
                        TlgAttachmentType.Photo, Option<UiString>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri2.html"), 
                        TlgAttachmentType.Photo, Option<UiString>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri3.html"), 
                        TlgAttachmentType.Voice, Option<UiString>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri4.html"), 
                        TlgAttachmentType.Document, Option<UiString>.None())
                } 
            }
        ];
        
        serviceCollection.AddScoped<IInputProcessorFactory>(_ =>
            GetMockInputProcessorFactoryWithSetUpReturnValue(outputWithMultipleAttachmentTypes, mode));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");

        await basics.handler.HandleUpdateAsync(update, mode);

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

    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    // This test passing implies that the main Text AND each attachment's caption are all seen by the user
    public async Task HandleUpdateAsync_SendsTextAndAttachments_ForOneOutputWithTextAndAttachments(InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        const string mainText = "This is the main text describing all attachments";
        
        List<OutputDto> outputWithTextAndCaptions =
        [
            new OutputDto
            {
                Text = UiNoTranslate(mainText),
                Attachments = new List<OutputAttachmentDetails>
                {
                    new(new Uri("http://www.gorin.de/fakeUri1.html"), 
                        TlgAttachmentType.Photo, Ui("Random caption for Attachment 1")),
                    new(new Uri("http://www.gorin.de/fakeUri2.html"), 
                        TlgAttachmentType.Photo, Ui("Random caption for Attachment 2")),
                }
            }
        ];
    
        serviceCollection.AddScoped<IInputProcessorFactory>(_ =>
            GetMockInputProcessorFactoryWithSetUpReturnValue(outputWithTextAndCaptions, mode));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(update, mode);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), 
                It.IsAny<string>(),
                mainText,
                It.IsAny<Option<IReplyMarkup>>(), 
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        basics.mockBotClient.Verify(
            x => x.SendPhotoAsync(
                It.IsAny<AttachmentSendOutParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
    
    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_SendsLocation_WhenOutputContainsOne(InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        List<OutputDto> outputWithLocation =
        [
            new OutputDto
            {
                Location = new Geo(35.098, -17.077, Option<float>.None()) 
            }
        ];
        
        serviceCollection.AddScoped<IInputProcessorFactory>(_ =>
            GetMockInputProcessorFactoryWithSetUpReturnValue(outputWithLocation, mode));
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage("random valid text");

        await basics.handler.HandleUpdateAsync(update, mode);
        
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
        Task<IEnumerable<TlgAgentRoleBind>> tlgAgentRoleBindingsTask)
        GetBasicTestingServices(IServiceProvider sp) => 
            (sp.GetRequiredService<ITestUtils>(), 
                sp.GetRequiredService<Mock<IBotClientWrapper>>(),
                sp.GetRequiredService<IUpdateHandler>(),
                sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>(),
                new UiTranslator(Option<IReadOnlyDictionary<string, string>>.None(), 
                    sp.GetRequiredService<ILogger<UiTranslator>>()),
                sp.GetRequiredService<ITlgAgentRoleBindingsRepository>().GetAllActiveAsync());

    // Useful when we need to mock up what ChatBot.Logic returns, e.g. to test ChatBot.Function related mechanics
    private static IInputProcessorFactory 
        GetMockInputProcessorFactoryWithSetUpReturnValue(
            IReadOnlyCollection<OutputDto> returnValue, InteractionMode interactionMode = Operations)
    {
        var mockInputProcessor = new Mock<IInputProcessor>();
        
        mockInputProcessor
            .Setup<Task<IReadOnlyCollection<OutputDto>>>(rp => 
                rp.ProcessInputAsync(It.IsAny<Result<TlgInput>>()))
            .Returns(Task.FromResult(returnValue));

        var mockInputProcessorFactory = new Mock<IInputProcessorFactory>();
        
        mockInputProcessorFactory
            .Setup(rps => 
                rps.GetInputProcessor(interactionMode))
            .Returns(mockInputProcessor.Object);

        return mockInputProcessorFactory.Object;
    }
}
