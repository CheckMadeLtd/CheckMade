using CheckMade.Common.LangExt;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
using CheckMade.Telegram.Model.DTOs;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Tests.Unit.Telegram.Function;

public class ToModelConverterTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData("Normal valid text message")]
    [InlineData("_")]
    [InlineData(" valid text message \n with line break and trailing spaces ")]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForValidTextMessage_ToAnyBotType(
        string textInput)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage(textInput);

        var expectedInputMessage = new InputMessageDto(
            update.Message.From!.Id,
            update.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.TextMessage,
            new InputMessageDetails(
                update.Message.Date,
                update.Message.MessageId,
                !string.IsNullOrWhiteSpace(update.Message.Text) 
                    ? update.Message.Text 
                    : Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<Geo>.None(), 
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));

        var actualInputMessage = 
            await basics.converter.ConvertToModelAsync(update, BotType.Operations);

        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }
    
    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForValidAttachmentMessage_ToAnyBotType(
        AttachmentType attachmentType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var attachmentUpdate = attachmentType switch
        {
            AttachmentType.Audio => basics.utils.GetValidTelegramAudioMessage(),
            AttachmentType.Document => basics.utils.GetValidTelegramDocumentMessage(),
            AttachmentType.Photo => basics.utils.GetValidTelegramPhotoMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var expectedAttachmentExternalUrl =
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{basics.mockBotClient.Object.BotToken}/" +
            $"{(await basics.mockBotClient.Object.GetFileOrThrowAsync("any")).FilePath}";

        var expectedInputMessage = new InputMessageDto(
            attachmentUpdate.Message.From!.Id,
            attachmentUpdate.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.AttachmentMessage,
            new InputMessageDetails(
                attachmentUpdate.Message.Date,
                attachmentUpdate.Message.MessageId,
                !string.IsNullOrWhiteSpace(attachmentUpdate.Message.Caption)
                    ? attachmentUpdate.Message.Caption
                    : Option<string>.None(),
                expectedAttachmentExternalUrl,
                attachmentType,
                Option<Geo>.None(), 
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));
        
        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            attachmentUpdate, BotType.Operations);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }

    [Theory]
    [InlineData(null)]
    [InlineData(500.23f)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForValidLocationMessage_ToAnyBotType(
        float? horizontalAccuracy)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var locationUpdate = basics.utils.GetValidTelegramLocationMessage(
            horizontalAccuracy ?? Option<float>.None());

        var location = locationUpdate.Message.Location;
        var expectedGeoCoordinates = new Geo(
            location!.Latitude,
            location.Longitude,
            horizontalAccuracy ?? Option<float>.None());
        
        var expectedInputMessage = new InputMessageDto(
                locationUpdate.Message.From!.Id,
                locationUpdate.Message.Chat.Id,
                BotType.Operations,
                ModelUpdateType.Location,
                new InputMessageDetails(
                    locationUpdate.Message.Date,
                    locationUpdate.Message.MessageId,
                    Option<string>.None(),
                    Option<string>.None(), 
                    Option<AttachmentType>.None(), 
                    expectedGeoCoordinates, 
                    Option<int>.None(),
                    Option<int>.None(), 
                    Option<long>.None()));
        
        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            locationUpdate, BotType.Operations);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }

    [Theory]
    [InlineData(OperationsBotCommands.NewIssue)]
    [InlineData(OperationsBotCommands.NewAssessment)]
    [InlineData(OperationsBotCommands.Settings)]
    [InlineData(OperationsBotCommands.Logout)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForBotCommandMessage_ToOperations(
        OperationsBotCommands command)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var operationsCommandMenu = 
            new BotCommandMenus().OperationsBotCommandMenu;
        var commandText = operationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(commandText);

        var expectedInputMessage = new InputMessageDto(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.CommandMessage,
            new InputMessageDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<Geo>.None(), 
                (int)command,
                Option<int>.None(),
                Option<long>.None()));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            commandUpdate, BotType.Operations);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());        
    }
    
    [Theory]
    [InlineData(CommunicationsBotCommands.Contact)]
    [InlineData(CommunicationsBotCommands.Settings)]
    [InlineData(CommunicationsBotCommands.Logout)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForBotCommandMessage_ToCommunications(
        CommunicationsBotCommands command)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var communicationsCommandMenu = 
            new BotCommandMenus().CommunicationsBotCommandMenu;
        var commandText = communicationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(commandText);

        var expectedInputMessage = new InputMessageDto(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Communications,
            ModelUpdateType.CommandMessage,
            new InputMessageDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<Geo>.None(), 
                (int)command,
                Option<int>.None(),
                Option<long>.None()));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            commandUpdate, BotType.Communications);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());        
    }

    [Theory]
    [InlineData(NotificationsBotCommands.Status)]
    [InlineData(NotificationsBotCommands.Settings)]
    [InlineData(NotificationsBotCommands.Logout)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForBotCommandMessage_ToNotifications(
        NotificationsBotCommands command)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var notificationsCommandMenu = 
            new BotCommandMenus().NotificationsBotCommandMenu;
        var commandText = notificationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(commandText);

        var expectedInputMessage = new InputMessageDto(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Notifications,
            ModelUpdateType.CommandMessage,
            new InputMessageDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<Geo>.None(), 
                (int)command,
                Option<int>.None(),
                Option<long>.None()));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            commandUpdate, BotType.Notifications);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());        
    }

    [Theory]
    [InlineData((long)DomainCategory.SanitaryOps_IssueCleanliness)]
    [InlineData((long)ControlPrompts.Good)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForMessageWithCallbackQuery_ToAnyBot(
        long enumSourceOfCallbackQuery)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var callbackQueryData = new EnumCallbackId(enumSourceOfCallbackQuery).Id;
        
        var callbackQuery = basics.utils.GetValidTelegramUpdateWithCallbackQuery(callbackQueryData);

        var domainCategoryEnumCode = enumSourceOfCallbackQuery <= EnumCallbackId.DomainCategoryMaxThreshold
            ? Option<int>.Some(int.Parse(callbackQuery.Update.CallbackQuery!.Data!))
            : Option<int>.None();

        var controlPromptEnumCode = enumSourceOfCallbackQuery > EnumCallbackId.DomainCategoryMaxThreshold
            ? Option<long>.Some(long.Parse(callbackQuery.Update.CallbackQuery!.Data!))
            : Option<long>.None();

        var expectedInputMessage = new InputMessageDto(
            callbackQuery.Message.From!.Id,
            callbackQuery.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.CallbackQuery,
            new InputMessageDetails(
                callbackQuery.Message.Date,
                callbackQuery.Message.MessageId,
                "The bot's original prompt",
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<Geo>.None(), 
                Option<int>.None(),
                domainCategoryEnumCode,
                controlPromptEnumCode));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
             callbackQuery, BotType.Operations);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenUserIsNull_ForAnyBotType()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
         var basics = GetBasicTestingServices(_services);
         
        var update = new UpdateWrapper(new Message { From = null, Text = "not empty" });
        var conversionAttempt = await basics.converter.ConvertToModelAsync(update, BotType.Operations);
        Assert.True(conversionAttempt.IsFailure);
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenTextAndAttachmentFileIdBothEmpty_ForAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = new UpdateWrapper(new Message { From = new User { Id = 123L } });
        
        var conversionAttempt = await basics.converter.ConvertToModelAsync(update, BotType.Operations);
        
        Assert.True(conversionAttempt.IsFailure);
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenUnsupportedAttachmentTypeLikeVoiceSent_ToAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var voiceMessage = basics.utils.GetValidTelegramVoiceMessage();
        var conversionAttempt = await basics.converter.ConvertToModelAsync(voiceMessage, BotType.Operations);

        Assert.True(conversionAttempt.IsFailure);
        Assert.Equal("Failed to convert Telegram Message to Model. Attachment type Voice is not yet supported!",
            conversionAttempt.Failure!.Error!.GetFormattedEnglish());
    }

    private static (ITestUtils utils, Mock<IBotClientWrapper> mockBotClient, IToModelConverter converter)
        GetBasicTestingServices(IServiceProvider sp)
    {
        var utils = sp.GetRequiredService<ITestUtils>();
        var mockBotClient = sp.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = sp.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));

        return (utils, mockBotClient, converter);
    }
}
