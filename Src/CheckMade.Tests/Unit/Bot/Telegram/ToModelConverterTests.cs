using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Core.Actors;
using CheckMade.Abstract.Domain.Model.Core.CrossCutting;
using CheckMade.Abstract.Domain.Model.Core.GIS;
using CheckMade.Abstract.Domain.Model.Core.LiveEvents;
using CheckMade.Abstract.Domain.ServiceInterfaces.Bot;
using CheckMade.Bot.Telegram.BotClient;
using CheckMade.Bot.Telegram.Conversion;
using CheckMade.Bot.Telegram.UpdateHandling;
using CheckMade.Tests.Startup;
using CheckMade.Tests.Utils;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using User = Telegram.Bot.Types.User;

namespace CheckMade.Tests.Unit.Bot.Telegram;

public sealed class ToModelConverterTests
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
        var agent = PrivateBotChat_Operations;
        var update = basics.updateGenerator.GetValidTelegramTextMessage(textInput);
        
        // based on defaultRoleBindings in TestRepositoryUtils
        var expectedOriginatorRole = SanitaryAdmin_DanielEn_X2024;
        var expectedLiveEventContext = 
            Option<ILiveEventInfo>.Some(SanitaryAdmin_DanielEn_X2024.AtLiveEvent); 
        
        var expectedInput = new Input(
            update.Message.Date,
            update.Message.MessageId,
            agent,
            InputType.TextMessage,
            expectedOriginatorRole,
            expectedLiveEventContext,
            Option<ResultantWorkflowState>.None(),
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                update.Message.Text));

        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                update, 
                agent.Mode);
        
        Assert.Equivalent(
            expectedInput, 
            actualInput.GetValueOrThrow());
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ConvertsWithNoOriginatorRoleInfo_ForInputFromExpiredRole()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var updateGenerator = _services.GetRequiredService<ITelegramUpdateGenerator>();
        var agent = UserId02_ChatId03_Operations;
        
        var update = updateGenerator.GetValidTelegramTextMessage(
            "valid text",
            agent.UserId,
            agent.ChatId);

        var serviceCollection = new UnitTestStartup().Services;
        var (services, _) = serviceCollection.ConfigureTestRepositories(
            roleBindings:
            [
                new AgentRoleBind(
                    SanitaryInspector_DanielEn_X2024,
                    agent,
                    new DateTime(2021, 01, 01),
                    Option<DateTimeOffset>.Some(new DateTime(2021, 01, 05)),
                    DbRecordStatus.Historic)
            ]);
        var basics = GetBasicTestingServices(services);
        
        var expectedInput = new Input(
            update.Message.Date,
            update.Message.MessageId,
            agent,
            InputType.TextMessage,
            Option<IRoleInfo>.None(), 
            Option<ILiveEventInfo>.None(), 
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                update.Message.Text));
    
        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                update, 
                agent.Mode);
    
        Assert.Equivalent(
            expectedInput, 
            actualInput.GetValueOrThrow());
        Assert.True(
            actualInput.GetValueOrThrow().OriginatorRole.IsNone);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(500.23f)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForValidLocationMessage_InAnyMode(
        float? horizontalAccuracy)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var locationUpdate = basics.updateGenerator.GetValidTelegramLocationMessage(horizontalAccuracy);
        var location = locationUpdate.Message.Location;
        
        var expectedGeoCoordinates = new Geo(
            location!.Latitude,
            location.Longitude,
            horizontalAccuracy ?? Option<double>.None());
        
        var expectedInput = new Input(
            locationUpdate.Message.Date,
            locationUpdate.Message.MessageId,
            PrivateBotChat_Operations,
            InputType.Location,
            SanitaryAdmin_DanielEn_X2024, 
            X2024, 
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                geoCoordinates: expectedGeoCoordinates));
        
        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                locationUpdate, 
                PrivateBotChat_Operations.Mode);
        
        Assert.Equivalent(
            expectedInput, 
            actualInput.GetValueOrThrow());
    }
    
    [Theory]
    [InlineData(OperationsBotCommands.NewSubmission)]
    // [InlineData(OperationsBotCommands.NewAssessment)]
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
    
        var expectedInput = new Input(
            commandUpdate.Message.Date,
            commandUpdate.Message.MessageId,
            PrivateBotChat_Operations,
            InputType.CommandMessage,
            SanitaryAdmin_DanielEn_X2024, 
            X2024, 
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                commandText,
                botCommandEnumCode: (int)command));
    
        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                commandUpdate,
                PrivateBotChat_Operations.Mode);
        
        Assert.Equivalent(
            expectedInput,
            actualInput.GetValueOrThrow());        
    }
    
    [Theory]
    // [InlineData(CommunicationsBotCommands.Contact)]
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
    
        var expectedInput = new Input(
            commandUpdate.Message.Date,
            commandUpdate.Message.MessageId,
            PrivateBotChat_Communications,
            InputType.CommandMessage,
            Option<IRoleInfo>.None(), 
            Option<ILiveEventInfo>.None(), 
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                commandText,
                botCommandEnumCode: (int)command));
    
        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                commandUpdate,
                PrivateBotChat_Communications.Mode);
        
        Assert.Equivalent(
            expectedInput,
            actualInput.GetValueOrThrow());        
    }
    
    [Theory]
    // [InlineData(NotificationsBotCommands.Status)]
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
    
        var expectedInput = new Input(
            commandUpdate.Message.Date,
            commandUpdate.Message.MessageId,
            PrivateBotChat_Notifications,
            InputType.CommandMessage,
            Option<IRoleInfo>.None(), 
            Option<ILiveEventInfo>.None(), 
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                commandText,
                botCommandEnumCode: (int)command));
    
        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                commandUpdate,
                PrivateBotChat_Notifications.Mode);
        
        Assert.Equivalent(
            expectedInput,
            actualInput.GetValueOrThrow());        
    }
    
    [Theory]
    [InlineData((long)ControlPrompts.Yes)]
    [InlineData((long)ControlPrompts.Submit)]
    public async Task ConvertToModelAsync_ConvertsCorrectly_ForMessageWithCallbackQueryToControlPrompt_InAnyMode(
        long enumSourceOfCallbackQuery)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var callbackQueryData = new CallbackId(enumSourceOfCallbackQuery);
        var callbackQuery = 
            basics.updateGenerator.GetValidTelegramUpdateWithCallbackQuery(callbackQueryData);
        var controlPromptEnumCode = (long?)long.Parse(callbackQuery.Update.CallbackQuery!.Data!);
    
        var expectedInput = new Input(
            callbackQuery.Message.Date,
            callbackQuery.Message.MessageId,
            PrivateBotChat_Operations,
            InputType.CallbackQuery,
            SanitaryAdmin_DanielEn_X2024, 
            X2024, 
            Option<ResultantWorkflowState>.None(),
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                "The bot's original prompt",
                controlPromptEnumCode: controlPromptEnumCode));
    
        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                callbackQuery,
                PrivateBotChat_Operations.Mode);
        
        Assert.Equivalent(
            expectedInput,
            actualInput.GetValueOrThrow());
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ConvertsCorrectly_ForMessageWithCallbackQueryDomainTerm_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        var basics = GetBasicTestingServices(_services);
        var domainTerm = Dt(LanguageCode.de);
        var callbackQueryData = new CallbackId(basics.glossary.GetId(domainTerm));
        var callbackQuery = 
            basics.updateGenerator.GetValidTelegramUpdateWithCallbackQuery(callbackQueryData);
    
        var expectedInput = new Input(
            callbackQuery.Message.Date,
            callbackQuery.Message.MessageId,
            PrivateBotChat_Operations,
            InputType.CallbackQuery,
            SanitaryAdmin_DanielEn_X2024, 
            X2024, 
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            Option<string>.None(), 
            InputGenerator.CreateFromRelevantDetails(
                "The bot's original prompt",
                domainTerm: domainTerm));
    
        var actualInput = 
            await basics.converter.ConvertToModelAsync(
                callbackQuery,
                PrivateBotChat_Operations.Mode);
        
        Assert.Equivalent(
            expectedInput,
            actualInput.GetValueOrThrow());
    }

    [Fact]
    public async Task ConvertToModelAsync_ThrowsArgumentNullException_WhenUserIsNull_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
     
        var message = new Message
        {
            From = null,
            Text = "not empty",
            Chat = new Chat{ Id = 1 },
            Id = 2,
            Date = DateTime.UtcNow
        };
    
        // It's the UpdateWrapper further up the stack that performs the null check and throws.
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            basics.converter.ConvertToModelAsync(
                new UpdateWrapper(message), 
                Operations)
        );
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenTextAndAttachmentFileIdBothEmpty_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var update = new UpdateWrapper(new Message
        {
            From = new User { Id = 123L },
            Chat = new Chat{ Id = 1 },
            Id = 2,
            Date = DateTime.UtcNow
        });
        
        var conversionResult = 
            await basics.converter.ConvertToModelAsync(
                update, 
                Operations);
        
        Assert.True(conversionResult.IsFailure);
    }

    private static (ITelegramUpdateGenerator updateGenerator, 
        Mock<IBotClientWrapper> mockBotClient,
        IToModelConverter converter,
        IDomainGlossary glossary)
        GetBasicTestingServices(IServiceProvider sp)
    {
        var updateGenerator = sp.GetRequiredService<ITelegramUpdateGenerator>();
        var mockBotClient = sp.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = sp.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));
        var glossary = sp.GetRequiredService<IDomainGlossary>();

        return (updateGenerator, mockBotClient, converter, glossary);
    }
}
