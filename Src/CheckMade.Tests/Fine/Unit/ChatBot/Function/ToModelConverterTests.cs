using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using User = Telegram.Bot.Types.User;

namespace CheckMade.Tests.Fine.Unit.ChatBot.Function;

public class ToModelConverterTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData("Normal valid text message")]
    [InlineData("_")]
    [InlineData(" valid text message \n with line break and trailing spaces ")]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForValidTextMessage_InAnyMode(
        string textInput)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var tlgAgent = TlgAgent_PrivateChat_Default;
        var update = basics.updateGenerator.GetValidTelegramTextMessage(textInput);
        var roleBindings = 
            (await basics.agentRoleBindingsRepo.GetAllActiveAsync())
            .ToImmutableReadOnlyCollection();
        
        var boundRole = roleBindings
            .FirstOrDefault(tarb => tarb.TlgAgent == tlgAgent)?
            .Role;
        
        // just confirming consistency of my internal TestData / TestUtils setup
        Assert.Equivalent(boundRole, SanitaryOpsAdmin_AtMockParooka2024_Default);

        var expectedTlgInput = new TlgInput(
            tlgAgent,
            TlgInputType.TextMessage,
            SanitaryOpsAdmin_AtMockParooka2024_Default,
            Option<ILiveEventInfo>.Some(
                SanitaryOpsAdmin_AtMockParooka2024_Default.AtLiveEvent),
            TlgInputGenerator.CreateFromRelevantDetails(
                update.Message.Date,
                update.Message.MessageId,
                update.Message.Text));

        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                update, 
                tlgAgent.Mode);
        
        Assert.Equivalent(
            expectedTlgInput, 
            actualTlgInput.GetValueOrThrow());
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ConvertsWithNoOriginatorRoleInfo_ForInputFromExpiredRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var tlgAgent = TlgAgent_HasOnly_HistoricRoleBind;
        var update = basics.updateGenerator.GetValidTelegramTextMessage(
            "valid text",
            tlgAgent.UserId,
            tlgAgent.ChatId);
        var agentRoleBindings = 
            (await basics.agentRoleBindingsRepo.GetAllActiveAsync())
            .ToImmutableReadOnlyCollection();
        
        var boundRole = agentRoleBindings
            .FirstOrDefault(tarb => tarb.TlgAgent == tlgAgent)?
            .Role;

        // just confirming consistency of my internal TestData / TestUtils setup
        Assert.Null(boundRole);

        var expectedTlgInput = new TlgInput(
            tlgAgent,
            TlgInputType.TextMessage,
            Option<IRoleInfo>.None(), 
            Option<ILiveEventInfo>.None(), 
            TlgInputGenerator.CreateFromRelevantDetails(
                update.Message.Date,
                update.Message.MessageId,
                update.Message.Text));

        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                update, 
                tlgAgent.Mode);

        Assert.Equivalent(
            expectedTlgInput, 
            actualTlgInput.GetValueOrThrow());
        Assert.True(actualTlgInput.GetValueOrThrow().OriginatorRole.IsNone);
    }
    
    [Theory]
    [InlineData(TlgAttachmentType.Photo)]
    [InlineData(TlgAttachmentType.Voice)]
    [InlineData(TlgAttachmentType.Document)]
    public async Task ConvertToModelAsync_ResultsInCorrectTlgUri_ForValidAttachmentMessage_InAnyMode(
        TlgAttachmentType attachmentType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var attachmentUpdate = attachmentType switch
        {
            TlgAttachmentType.Document => basics.updateGenerator.GetValidTelegramDocumentMessage(),
            TlgAttachmentType.Photo => basics.updateGenerator.GetValidTelegramPhotoMessage(),
            TlgAttachmentType.Voice => basics.updateGenerator.GetValidTelegramVoiceMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var expectedAttachmentTlgUri = new Uri(
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + 
            $"bot{basics.mockBotClient.Object.MyBotToken}/" +
            $"{(await basics.mockBotClient.Object.GetFileAsync("any")).FilePath}");
    
        var expectedTlgInput = new TlgInput(
            TlgAgent_PrivateChat_Default,
            TlgInputType.AttachmentMessage,
            SanitaryOpsAdmin_AtMockParooka2024_Default, 
            Option<ILiveEventInfo>.Some(
                SanitaryOpsAdmin_AtMockParooka2024_Default.AtLiveEvent), 
            TlgInputGenerator.CreateFromRelevantDetails(
                attachmentUpdate.Message.Date,
                attachmentUpdate.Message.MessageId,
                attachmentUpdate.Message.Caption,
                expectedAttachmentTlgUri,
                new Uri("https://gorin.de/Can_test_for_this_only_in_integration_tests"),
                attachmentType));
        
        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                attachmentUpdate, 
                TlgAgent_PrivateChat_Default.Mode);
        
        // Can't do a deep comparison with Equivalent on the entire input here due to the complex Uri() type.
        Assert.Equal(
            expectedTlgInput.Details.AttachmentTlgUri.GetValueOrThrow().AbsoluteUri, 
            actualTlgInput.GetValueOrThrow().Details.AttachmentTlgUri.GetValueOrThrow().AbsoluteUri);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(500.23f)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForValidLocationMessage_InAnyMode(
        float? horizontalAccuracy)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var locationUpdate = basics.updateGenerator.GetValidTelegramLocationMessage(
            horizontalAccuracy ?? Option<float>.None());
    
        var location = locationUpdate.Message.Location;
        var expectedGeoCoordinates = new Geo(
            location!.Latitude,
            location.Longitude,
            horizontalAccuracy ?? Option<float>.None());
        
        var expectedTlgInput = new TlgInput(
                TlgAgent_PrivateChat_Default,
                TlgInputType.Location,
                SanitaryOpsAdmin_AtMockParooka2024_Default, 
                MockParooka2024, 
                TlgInputGenerator.CreateFromRelevantDetails(
                    locationUpdate.Message.Date,
                    locationUpdate.Message.MessageId,
                    geoCoordinates: expectedGeoCoordinates));
        
        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                locationUpdate, 
                TlgAgent_PrivateChat_Default.Mode);
        
        Assert.Equivalent(
            expectedTlgInput, 
            actualTlgInput.GetValueOrThrow());
    }
    
    [Theory]
    [InlineData(OperationsBotCommands.NewIssue)]
    [InlineData(OperationsBotCommands.NewAssessment)]
    [InlineData(OperationsBotCommands.Settings)]
    [InlineData(OperationsBotCommands.Logout)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForBotCommandMessage_InOperationsMode(
        OperationsBotCommands command)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var operationsCommandMenu = 
            new BotCommandMenus().OperationsBotCommandMenu;
        var commandText = operationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.updateGenerator.GetValidTelegramBotCommandMessage(commandText);
    
        var expectedTlgInput = new TlgInput(
            TlgAgent_PrivateChat_Default,
            TlgInputType.CommandMessage,
            SanitaryOpsAdmin_AtMockParooka2024_Default, 
            MockParooka2024, 
            TlgInputGenerator.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));
    
        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                commandUpdate,
                TlgAgent_PrivateChat_Default.Mode);
        
        Assert.Equivalent(
            expectedTlgInput,
            actualTlgInput.GetValueOrThrow());        
    }
    
    [Theory]
    [InlineData(CommunicationsBotCommands.Contact)]
    [InlineData(CommunicationsBotCommands.Settings)]
    [InlineData(CommunicationsBotCommands.Logout)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForBotCommandMessage_InCommunicationsMode(
        CommunicationsBotCommands command)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var communicationsCommandMenu = 
            new BotCommandMenus().CommunicationsBotCommandMenu;
        var commandText = communicationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.updateGenerator.GetValidTelegramBotCommandMessage(commandText);
    
        var expectedTlgInput = new TlgInput(
            TlgAgent_PrivateChat_CommunicationsMode,
            TlgInputType.CommandMessage,
            SanitaryOpsAdmin_AtMockParooka2024_Default, 
            MockParooka2024, 
            TlgInputGenerator.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));
    
        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                commandUpdate,
                TlgAgent_PrivateChat_CommunicationsMode.Mode);
        
        Assert.Equivalent(
            expectedTlgInput,
            actualTlgInput.GetValueOrThrow());        
    }
    
    [Theory]
    [InlineData(NotificationsBotCommands.Status)]
    [InlineData(NotificationsBotCommands.Settings)]
    [InlineData(NotificationsBotCommands.Logout)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForBotCommandMessage_InNotificationsMode(
        NotificationsBotCommands command)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var notificationsCommandMenu = 
            new BotCommandMenus().NotificationsBotCommandMenu;
        var commandText = notificationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.updateGenerator.GetValidTelegramBotCommandMessage(commandText);
    
        var expectedTlgInput = new TlgInput(
            TlgAgent_PrivateChat_NotificationsMode,
            TlgInputType.CommandMessage,
            SanitaryOpsAdmin_AtMockParooka2024_Default, 
            MockParooka2024, 
            TlgInputGenerator.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));
    
        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                commandUpdate,
                TlgAgent_PrivateChat_NotificationsMode.Mode);
        
        Assert.Equivalent(
            expectedTlgInput,
            actualTlgInput.GetValueOrThrow());        
    }
    
    [Theory]
    [InlineData((long)ControlPrompts.Good)]
    [InlineData((long)ControlPrompts.Submit)]
    public async Task ConvertToModelAsync_ConvertsCorrectly_ForMessageWithCallbackQueryToControlPrompt_InAnyMode(
        long enumSourceOfCallbackQuery)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var callbackQueryData = new CallbackId(enumSourceOfCallbackQuery);
        var callbackQuery = basics.updateGenerator.GetValidTelegramUpdateWithCallbackQuery(callbackQueryData);
        var controlPromptEnumCode = (long?)long.Parse(callbackQuery.Update.CallbackQuery!.Data!);
    
        var expectedTlgInput = new TlgInput(
            TlgAgent_PrivateChat_Default,
            TlgInputType.CallbackQuery,
            SanitaryOpsAdmin_AtMockParooka2024_Default, 
            MockParooka2024, 
            TlgInputGenerator.CreateFromRelevantDetails(
                callbackQuery.Message.Date,
                callbackQuery.Message.MessageId,
                "The bot's original prompt",
                controlPromptEnumCode: controlPromptEnumCode));
    
        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                callbackQuery,
                TlgAgent_PrivateChat_Default.Mode);
        
        Assert.Equivalent(
            expectedTlgInput,
            actualTlgInput.GetValueOrThrow());
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ConvertsCorrectly_ForMessageWithCallbackQueryDomainTerm_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var domainGlossary = new DomainGlossary();
        var domainTerm = Dt(LanguageCode.de);
        var callbackQueryData = new CallbackId(domainGlossary.IdAndUiByTerm[domainTerm].callbackId);
        var callbackQuery = basics.updateGenerator.GetValidTelegramUpdateWithCallbackQuery(callbackQueryData);
    
        var expectedTlgInput = new TlgInput(
            TlgAgent_PrivateChat_Default,
            TlgInputType.CallbackQuery,
            SanitaryOpsAdmin_AtMockParooka2024_Default, 
            MockParooka2024, 
            TlgInputGenerator.CreateFromRelevantDetails(
                callbackQuery.Message.Date,
                callbackQuery.Message.MessageId,
                "The bot's original prompt",
                domainTerm: domainTerm));
    
        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(
                callbackQuery,
                TlgAgent_PrivateChat_Default.Mode);
        
        Assert.Equivalent(
            expectedTlgInput,
            actualTlgInput.GetValueOrThrow());
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenUserIsNull_InAnyMode()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
         var basics = GetBasicTestingServices(_services);
         
        var update = new UpdateWrapper(new Message
        {
            From = null,
            Text = "not empty",
            Chat = new Chat{ Id = 1 },
            MessageId = 2,
            Date = DateTime.UtcNow
        });
        
        var conversionResult = 
            await basics.converter.ConvertToModelAsync(
                update, 
                Operations);
        
        Assert.True(conversionResult.IsError);
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenTextAndAttachmentFileIdBothEmpty_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = new UpdateWrapper(new Message
        {
            From = new User { Id = 123L },
            Chat = new Chat{ Id = 1 },
            MessageId = 2,
            Date = DateTime.UtcNow
        });
        
        var conversionResult = 
            await basics.converter.ConvertToModelAsync(
                update, 
                Operations);
        
        Assert.True(conversionResult.IsError);
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenUnsupportedAttachmentTypeLikeAudioSent_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var audioMessage = basics.updateGenerator.GetValidTelegramAudioMessage();
        var conversionResult = 
            await basics.converter.ConvertToModelAsync(
                audioMessage, 
                Operations);

        Assert.True(conversionResult.IsError);
        Assert.Equal(
            "Failed to convert your Telegram Message: Attachment type Audio is not yet supported!",
            conversionResult.Error!.GetFormattedEnglish());
    }

    private static (ITelegramUpdateGenerator updateGenerator, Mock<IBotClientWrapper> mockBotClient, IToModelConverter converter, ITlgAgentRoleBindingsRepository agentRoleBindingsRepo)
        GetBasicTestingServices(IServiceProvider sp)
    {
        var updateGenerator = sp.GetRequiredService<ITelegramUpdateGenerator>();
        var mockBotClient = sp.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = sp.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));
        var agentRoleBindingsRepo = sp.GetRequiredService<ITlgAgentRoleBindingsRepository>();

        return (updateGenerator, mockBotClient, converter, agentRoleBindingsRepo);
    }
}
