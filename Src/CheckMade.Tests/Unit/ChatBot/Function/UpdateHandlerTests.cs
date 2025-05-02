using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Xunit.Abstractions;
using MessageType = Telegram.Bot.Types.Enums.MessageType;
using static CheckMade.Tests.Utils.TestUtils;

namespace CheckMade.Tests.Unit.ChatBot.Function;

public sealed class UpdateHandlerTests(ITestOutputHelper outputHelper)
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
        
        mockLogger.Verify(
            l => l.Log(
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
        
        var mockInputProcessor = new Mock<IInputProcessor>();
        mockInputProcessor
            .Setup(static opr => opr.ProcessInputAsync(It.IsAny<Result<TlgInput>>()))
            .Throws<Exception>();
        serviceCollection.AddScoped<IInputProcessor>(_ => mockInputProcessor.Object);
        
        var mockLogger = new Mock<ILogger<UpdateHandler>>();
        serviceCollection.AddScoped<ILogger<UpdateHandler>>(_ => mockLogger.Object);
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.updateGenerator.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(textUpdate, mode);
        
        mockLogger.Verify(
            static l => l.Log(
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
        var invalidBotCommandUpdate = basics.updateGenerator.GetValidTelegramBotCommandMessage(invalidBotCommand);
        const string expectedErrorCode = "W3DL9";
    
        // Writing out to OutputHelper to see the entire error message, as an additional manual verification
        basics.mockBotClient
            .Setup(x => x.SendTextMessageAsync(
                invalidBotCommandUpdate.Message.Chat.Id,
                It.IsAny<string>(),
                It.IsAny<string>(),
                Option<ReplyMarkup>.None(),
                It.IsAny<CancellationToken>()))
            .Callback<ChatId, string, string, Option<ReplyMarkup>, CancellationToken>((_, msg, _, _, _) => 
                outputHelper.WriteLine(msg));
        
        await basics.handler.HandleUpdateAsync(invalidBotCommandUpdate, Operations);
    
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                invalidBotCommandUpdate.Message.Chat.Id,
                It.IsAny<string>(),
                It.Is<string>(static msg => msg.Contains(expectedErrorCode)),
                Option<ReplyMarkup>.None(),
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task HandleUpdateAsync_ReturnsEnglishTestString_ForEnglishSpeakingUser()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        serviceCollection.AddScoped<IInputProcessor>(static _ => 
            GetStubInputProcessor( 
                new List<OutputDto>
                {
                    new() { Text = EnglishUiStringForTests }
                }));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var updateFromEnglishUser = 
            basics.updateGenerator.GetValidTelegramTextMessage("any valid text");
        
        await basics.handler.HandleUpdateAsync(
            updateFromEnglishUser,
            PrivateBotChat_Operations.Mode);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                updateFromEnglishUser.Message.Chat.Id,
                It.IsAny<string>(),
                EnglishUiStringForTests.GetFormattedEnglish(), // untranslated English message expected
                Option<ReplyMarkup>.None(),
                It.IsAny<CancellationToken>()));
    }
    
    [Fact]
    public async Task HandleUpdateAsync_ReturnsGermanTestString_ForGermanSpeakingUser()
    {
        var serviceCollection = new UnitTestStartup().Services;
        
        serviceCollection.AddScoped<IInputProcessor>(static _ => 
            GetStubInputProcessor(
                new List<OutputDto>
                {
                    new() { Text = EnglishUiStringForTests }
                }));
        
        var tlgAgent = UserId02_ChatId04_Operations;
        
        var (repoServices, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings:
            [
                TestRepositoryUtils.GetNewRoleBind(
                    SanitaryInspector_DanielDe_X2024,
                    tlgAgent)
            ]); 
        
        _services = repoServices;
        var basics = GetBasicTestingServices(_services);
        
        var updateFromGermanUser = 
            basics.updateGenerator.GetValidTelegramTextMessage(
                "any valid text",
                tlgAgent.UserId,
                tlgAgent.ChatId);
        
        await basics.handler.HandleUpdateAsync(
            updateFromGermanUser,
            tlgAgent.Mode);
        
        basics.mockBotClient.Verify(
            x => x.SendTextMessageAsync(
                updateFromGermanUser.Message.Chat.Id,
                It.IsAny<string>(),
                GermanStringForTests, // German translation expected
                Option<ReplyMarkup>.None(),
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
        
        List<OutputDto> outputWithPrompts = 
        [ 
            new()
            {
                Text = EnglishUiStringForTests,
                ControlPromptsSelection = ControlPrompts.Yes | ControlPrompts.No 
            }
        ];
        
        serviceCollection.AddScoped<IInputProcessor>(_ => 
            GetStubInputProcessor(outputWithPrompts));
    
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var textUpdate = basics.updateGenerator.GetValidTelegramTextMessage("any valid text");
        var converter = basics.markupConverterFactory.Create(basics.emptyTranslator);
        var expectedReplyMarkup = converter.GetReplyMarkup(outputWithPrompts[0]);
        
        var actualMarkup = Option<ReplyMarkup>.None();
        basics.mockBotClient
            .Setup(
                static x => x.SendTextMessageAsync(
                    It.IsAny<ChatId>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Option<ReplyMarkup>>(),
                    It.IsAny<CancellationToken>())
            )
            .Callback<ChatId, string, string, Option<ReplyMarkup>, CancellationToken>(
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
        
        List<OutputDto> outputsMultiple = 
        [
            new OutputDto { Text = UiNoTranslate("Output1") },
            new OutputDto { Text = UiNoTranslate("Output2") }
        ];
        
        serviceCollection.AddScoped<IInputProcessor>(_ => 
            GetStubInputProcessor(outputsMultiple));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.updateGenerator.GetValidTelegramTextMessage("any valid text");

        await basics.handler.HandleUpdateAsync(update, mode);
        
        basics.mockBotClient.Verify(
            static x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<Option<ReplyMarkup>>(), 
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
        
        List<OutputDto> outputsWithLogicalPort = 
        [
            new OutputDto
            { 
                LogicalPort = new LogicalPort(
                    SanitaryInspector_DanielEn_X2024, 
                    Operations), 
                Text = UiNoTranslate("Output1")   
            },
            new OutputDto
            {
                LogicalPort = new LogicalPort(
                    SanitaryTeamLead_DanielEn_X2024, 
                    Notifications),
                Text = UiNoTranslate("Output2") 
            },
            new OutputDto
            {
                LogicalPort = new LogicalPort(
                    SanitaryEngineer_DanielEn_X2024, 
                    Communications),
                Text = UiNoTranslate("Output3)") 
            }
        ];
        
        serviceCollection.AddScoped<IInputProcessor>(_ => 
            GetStubInputProcessor(outputsWithLogicalPort));
    
        List<TlgAgentRoleBind> activeRoleBindings = [ 
            TestRepositoryUtils.GetNewRoleBind(SanitaryInspector_DanielEn_X2024, PrivateBotChat_Operations),
            TestRepositoryUtils.GetNewRoleBind(SanitaryTeamLead_DanielEn_X2024, PrivateBotChat_Notifications),
            TestRepositoryUtils.GetNewRoleBind(SanitaryEngineer_DanielEn_X2024, PrivateBotChat_Communications)];
        
        var (repoServices, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings: activeRoleBindings);
        
        _services = repoServices;
        var basics = GetBasicTestingServices(_services);
        var update = basics.updateGenerator.GetValidTelegramTextMessage("any valid text");
        
        var expectedSendParamSets = outputsWithLogicalPort
            .Select(output => new 
            {
                Text = output.Text.GetValueOrThrow().GetFormattedEnglish(),
                
                TlgChatId = activeRoleBindings
                    .First(tarb => 
                        tarb.Role.Equals(output.LogicalPort.GetValueOrThrow().Role) &&
                        tarb.TlgAgent.Mode.Equals(output.LogicalPort.GetValueOrThrow().InteractionMode))
                    .TlgAgent.ChatId.Id
            }).ToList();
    
        await basics.handler.HandleUpdateAsync(update, mode);
    
        foreach (var expectedParamSet in expectedSendParamSets)
        {
            basics.mockBotClient.Verify(
                x => x.SendTextMessageAsync(
                    expectedParamSet.TlgChatId,
                    It.IsAny<string>(),
                    expectedParamSet.Text,
                    It.IsAny<Option<ReplyMarkup>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
    
    [Theory]
    [InlineData(Operations)]
    [InlineData(Communications)]
    [InlineData(Notifications)]
    public async Task HandleUpdateAsync_SendsToCurrentChatId_WhenOutputDtoHasNoLogicalPort(
        InteractionMode mode)
    {
        var serviceCollection = new UnitTestStartup().Services;
        const string expectedOutputMessage = "Output without logical port";
        List<OutputDto> outputWithoutPort = [new OutputDto{ Text = UiNoTranslate(expectedOutputMessage) }];
    
        serviceCollection.AddScoped<IInputProcessor>(_ => 
            GetStubInputProcessor((outputWithoutPort)));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        const long expectedChatId = ChatId04;
        var update = basics.updateGenerator.GetValidTelegramTextMessage(
            "any valid text",
            UserId02,
            expectedChatId);
    
        await basics.handler.HandleUpdateAsync(update, mode);
        
        basics.mockBotClient.Verify(
            static x => x.SendTextMessageAsync(
                expectedChatId,
                It.IsAny<string>(),
                expectedOutputMessage,
                It.IsAny<Option<ReplyMarkup>>(),
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
                Attachments = new List<AttachmentDetails>
                {
                    new(new Uri("https://www.gorin.de/fakeUri1.html"), 
                        TlgAttachmentType.Photo, Option<string>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri2.html"), 
                        TlgAttachmentType.Photo, Option<string>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri3.html"), 
                        TlgAttachmentType.Voice, Option<string>.None()),
                    new(new Uri("https://www.gorin.de/fakeUri4.html"), 
                        TlgAttachmentType.Document, Option<string>.None())
                } 
            }
        ];
        
        serviceCollection.AddScoped<IInputProcessor>(_ =>
            GetStubInputProcessor(outputWithMultipleAttachmentTypes));
        
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.updateGenerator.GetValidTelegramTextMessage("random valid text");
    
        await basics.handler.HandleUpdateAsync(update, mode);
    
        basics.mockBotClient.Verify(
            static x => x.SendPhotoAsync(
                It.IsAny<AttachmentSendOutParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    
        basics.mockBotClient.Verify(
            static x => x.SendVoiceAsync(
                It.IsAny<AttachmentSendOutParameters>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(1));
        
        basics.mockBotClient.Verify(
            static x => x.SendDocumentAsync(
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
                Attachments = new List<AttachmentDetails>
                {
                    new(new Uri("http://www.gorin.de/fakeUri1.html"), 
                        TlgAttachmentType.Photo, "Random caption for Attachment 1"),
                    new(new Uri("http://www.gorin.de/fakeUri2.html"), 
                        TlgAttachmentType.Photo, "Random caption for Attachment 2"),
                }
            }
        ];
    
        serviceCollection.AddScoped<IInputProcessor>(_ => 
            GetStubInputProcessor(outputWithTextAndCaptions));
    
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.updateGenerator.GetValidTelegramTextMessage("random valid text");
        
        await basics.handler.HandleUpdateAsync(update, mode);
        
        basics.mockBotClient.Verify(
            static x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(), 
                It.IsAny<string>(),
                mainText,
                It.IsAny<Option<ReplyMarkup>>(), 
                It.IsAny<CancellationToken>()),
            Times.Once);
        
        basics.mockBotClient.Verify(
            static x => x.SendPhotoAsync(
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
                Location = new Geo(35.098, -17.077, Option<double>.None()) 
            }
        ];
        
        serviceCollection.AddScoped<IInputProcessor>(_ => 
            GetStubInputProcessor(outputWithLocation));
    
        _services = serviceCollection.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.updateGenerator.GetValidTelegramTextMessage("random valid text");
    
        await basics.handler.HandleUpdateAsync(update, mode);
        
        basics.mockBotClient.Verify(
            x => x.SendLocationAsync(
                It.IsAny<ChatId>(),
                It.Is<Geo>(geo => geo == outputWithLocation[0].Location),
                It.IsAny<Option<ReplyMarkup>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static (ITelegramUpdateGenerator updateGenerator,
        ITlgInputGenerator inputGenerator,
        Mock<IBotClientWrapper> mockBotClient,
        IUpdateHandler handler,
        IOutputToReplyMarkupConverterFactory markupConverterFactory,
        IUiTranslator emptyTranslator)
        GetBasicTestingServices(IServiceProvider sp)
    {
        var mockBotClient = sp.GetRequiredService<Mock<IBotClientWrapper>>();
        
        mockBotClient
            .Setup(static x => x.SendTextMessageAsync(
                It.IsAny<ChatId>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Option<ReplyMarkup>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new TlgMessageId(1));
        
        return (sp.GetRequiredService<ITelegramUpdateGenerator>(),
            sp.GetRequiredService<ITlgInputGenerator>(),
            mockBotClient,
            sp.GetRequiredService<IUpdateHandler>(),
            sp.GetRequiredService<IOutputToReplyMarkupConverterFactory>(),
            new UiTranslator(Option<IReadOnlyDictionary<string, string>>.None(), 
                sp.GetRequiredService<ILogger<UiTranslator>>()));
    } 
    
    private static IInputProcessor GetStubInputProcessor(IReadOnlyCollection<OutputDto> returningOutputs)
    {
        var sp = new UnitTestStartup().Services.BuildServiceProvider();
        var inputGenerator = sp.GetRequiredService<ITlgInputGenerator>();
        var validInput = inputGenerator.GetValidTlgInputTextMessage();
        
        var mockInputProcessor = new Mock<IInputProcessor>();
        
        mockInputProcessor
            .Setup<Task<(Option<TlgInput> EnrichedOriginalInput, IReadOnlyCollection<OutputDto> ResultingOutputs)>>(
                static ip => ip.ProcessInputAsync(It.IsAny<Result<TlgInput>>()))
            .ReturnsAsync((validInput, returningOutputs));

        return mockInputProcessor.Object;
    }
}
