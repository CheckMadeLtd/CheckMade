using CheckMade.Common.LangExt;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByBotType;
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

        var expectedTelegramUpdate = new TelegramUpdate(
            update.Message.From!.Id,
            update.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.TextMessage,
            TestUtils.CreateFromRelevantDetails(
                update.Message.Date,
                update.Message.MessageId,
                update.Message.Text));

        var actualTelegramUpdate = 
            await basics.converter.ConvertToModelAsync(update, BotType.Operations);

        Assert.Equivalent(expectedTelegramUpdate, actualTelegramUpdate.GetValueOrDefault());
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
        
        var expectedAttachmentTelegramUri = new Uri(
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{basics.mockBotClient.Object.MyBotToken}/" +
            $"{(await basics.mockBotClient.Object.GetFileOrThrowAsync("any")).FilePath}");

        var expectedTelegramUpdate = new TelegramUpdate(
            attachmentUpdate.Message.From!.Id,
            attachmentUpdate.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.AttachmentMessage,
            TestUtils.CreateFromRelevantDetails(
                attachmentUpdate.Message.Date,
                attachmentUpdate.Message.MessageId,
                attachmentUpdate.Message.Caption,
                expectedAttachmentTelegramUri,
                new Uri("https://gorin.de/Can_test_for_this_only_in_integration_tests"),
                attachmentType));
        
        var actualTelegramUpdate = await basics.converter.ConvertToModelAsync(
            attachmentUpdate, BotType.Operations);
        
        Assert.Equivalent(expectedTelegramUpdate.Details.AttachmentTelegramUri.GetValueOrDefault().AbsoluteUri, 
            actualTelegramUpdate.GetValueOrDefault().Details.AttachmentTelegramUri.GetValueOrDefault().AbsoluteUri);
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
        
        var expectedTelegramUpdate = new TelegramUpdate(
                locationUpdate.Message.From!.Id,
                locationUpdate.Message.Chat.Id,
                BotType.Operations,
                ModelUpdateType.Location,
                TestUtils.CreateFromRelevantDetails(
                    locationUpdate.Message.Date,
                    locationUpdate.Message.MessageId,
                    geoCoordinates: expectedGeoCoordinates));
        
        var actualTelegramUpdate = await basics.converter.ConvertToModelAsync(
            locationUpdate, BotType.Operations);
        
        Assert.Equivalent(expectedTelegramUpdate, actualTelegramUpdate.GetValueOrDefault());
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

        var expectedTelegramUpdate = new TelegramUpdate(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTelegramUpdate = await basics.converter.ConvertToModelAsync(
            commandUpdate, BotType.Operations);
        
        Assert.Equivalent(expectedTelegramUpdate, actualTelegramUpdate.GetValueOrDefault());        
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

        var expectedTelegramUpdate = new TelegramUpdate(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Communications,
            ModelUpdateType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTelegramUpdate = await basics.converter.ConvertToModelAsync(
            commandUpdate, BotType.Communications);
        
        Assert.Equivalent(expectedTelegramUpdate, actualTelegramUpdate.GetValueOrDefault());        
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

        var expectedTelegramUpdate = new TelegramUpdate(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Notifications,
            ModelUpdateType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTelegramUpdate = await basics.converter.ConvertToModelAsync(
            commandUpdate, BotType.Notifications);
        
        Assert.Equivalent(expectedTelegramUpdate, actualTelegramUpdate.GetValueOrDefault());        
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
            ? (int?) int.Parse(callbackQuery.Update.CallbackQuery!.Data!)
            : null;

        var controlPromptEnumCode = enumSourceOfCallbackQuery > EnumCallbackId.DomainCategoryMaxThreshold
            ? (long?) long.Parse(callbackQuery.Update.CallbackQuery!.Data!)
            : null;

        var expectedTelegramUpdate = new TelegramUpdate(
            callbackQuery.Message.From!.Id,
            callbackQuery.Message.Chat.Id,
            BotType.Operations,
            ModelUpdateType.CallbackQuery,
            TestUtils.CreateFromRelevantDetails(
                callbackQuery.Message.Date,
                callbackQuery.Message.MessageId,
                "The bot's original prompt",
                domainCategoryEnumCode: domainCategoryEnumCode,
                controlPromptEnumCode: controlPromptEnumCode));

        var actualTelegramUpdate = await basics.converter.ConvertToModelAsync(
             callbackQuery, BotType.Operations);
        
        Assert.Equivalent(expectedTelegramUpdate, actualTelegramUpdate.GetValueOrDefault());
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenUserIsNull_ForAnyBotType()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
         var basics = GetBasicTestingServices(_services);
         
        var update = new UpdateWrapper(new Message { From = null, Text = "not empty" });
        var conversionAttempt = await basics.converter.ConvertToModelAsync(update, BotType.Operations);
        Assert.True(conversionAttempt.IsError);
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenTextAndAttachmentFileIdBothEmpty_ForAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = new UpdateWrapper(new Message { From = new User { Id = 123L } });
        
        var conversionAttempt = await basics.converter.ConvertToModelAsync(update, BotType.Operations);
        
        Assert.True(conversionAttempt.IsError);
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenUnsupportedAttachmentTypeLikeVoiceSent_ToAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var voiceMessage = basics.utils.GetValidTelegramVoiceMessage();
        var conversionAttempt = await basics.converter.ConvertToModelAsync(voiceMessage, BotType.Operations);

        Assert.True(conversionAttempt.IsError);
        Assert.Equal("Failed to convert Telegram Message to Model. Attachment type Voice is not yet supported!",
            conversionAttempt.Error!.FailureMessage!.GetFormattedEnglish());
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
