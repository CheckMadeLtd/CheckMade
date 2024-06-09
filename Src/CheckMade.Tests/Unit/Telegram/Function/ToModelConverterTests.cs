using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Enums.UserInteraction;
using CheckMade.Common.Model.Enums.UserInteraction.Helpers;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversion;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Model.BotCommand;
using CheckMade.Telegram.Model.BotCommand.DefinitionsByInteractionMode;
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
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForValidTextMessage_InAnyMode(
        string textInput)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = basics.utils.GetValidTelegramTextMessage(textInput);

        var expectedTlgInput = new TlgInput(
            update.Message.From!.Id,
            update.Message.Chat.Id,
            InteractionMode.Operations,
            TlgInputType.TextMessage,
            TestUtils.CreateFromRelevantDetails(
                update.Message.Date,
                update.Message.MessageId,
                update.Message.Text));

        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(update, InteractionMode.Operations);

        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());
    }
    
    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Voice)]
    [InlineData(AttachmentType.Document)]
    public async Task ConvertToModelAsync_ResultsInCorrectTlgUri_ForValidAttachmentMessage_InAnyMode(
        AttachmentType attachmentType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var attachmentUpdate = attachmentType switch
        {
            AttachmentType.Document => basics.utils.GetValidTelegramDocumentMessage(),
            AttachmentType.Photo => basics.utils.GetValidTelegramPhotoMessage(),
            AttachmentType.Voice => basics.utils.GetValidTelegramVoiceMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var expectedAttachmentTlgUri = new Uri(
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{basics.mockBotClient.Object.MyBotToken}/" +
            $"{(await basics.mockBotClient.Object.GetFileAsync("any")).FilePath}");

        var expectedTlgInput = new TlgInput(
            attachmentUpdate.Message.From!.Id,
            attachmentUpdate.Message.Chat.Id,
            InteractionMode.Operations,
            TlgInputType.AttachmentMessage,
            TestUtils.CreateFromRelevantDetails(
                attachmentUpdate.Message.Date,
                attachmentUpdate.Message.MessageId,
                attachmentUpdate.Message.Caption,
                expectedAttachmentTlgUri,
                new Uri("https://gorin.de/Can_test_for_this_only_in_integration_tests"),
                attachmentType));
        
        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            attachmentUpdate, InteractionMode.Operations);
        
        // Can't do a deep comparison with Equivalent on the entire input here due to the complex Uri() type.
        Assert.Equal(expectedTlgInput.Details.AttachmentTlgUri.GetValueOrThrow().AbsoluteUri, 
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
        var locationUpdate = basics.utils.GetValidTelegramLocationMessage(
            horizontalAccuracy ?? Option<float>.None());

        var location = locationUpdate.Message.Location;
        var expectedGeoCoordinates = new Geo(
            location!.Latitude,
            location.Longitude,
            horizontalAccuracy ?? Option<float>.None());
        
        var expectedTlgInput = new TlgInput(
                locationUpdate.Message.From!.Id,
                locationUpdate.Message.Chat.Id,
                InteractionMode.Operations,
                TlgInputType.Location,
                TestUtils.CreateFromRelevantDetails(
                    locationUpdate.Message.Date,
                    locationUpdate.Message.MessageId,
                    geoCoordinates: expectedGeoCoordinates));
        
        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            locationUpdate, InteractionMode.Operations);
        
        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());
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
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(commandText);

        var expectedTlgInput = new TlgInput(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            InteractionMode.Operations,
            TlgInputType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            commandUpdate, InteractionMode.Operations);
        
        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());        
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
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(commandText);

        var expectedTlgInput = new TlgInput(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            InteractionMode.Communications,
            TlgInputType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            commandUpdate, InteractionMode.Communications);
        
        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());        
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
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(commandText);

        var expectedTlgInput = new TlgInput(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            InteractionMode.Notifications,
            TlgInputType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            commandUpdate, InteractionMode.Notifications);
        
        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());        
    }

    [Theory]
    [InlineData((long)DomainCategory.SanitaryOps_IssueCleanliness)]
    [InlineData((long)ControlPrompts.Good)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForMessageWithCallbackQuery_InAnyMode(
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

        var expectedTlgInput = new TlgInput(
            callbackQuery.Message.From!.Id,
            callbackQuery.Message.Chat.Id,
            InteractionMode.Operations,
            TlgInputType.CallbackQuery,
            TestUtils.CreateFromRelevantDetails(
                callbackQuery.Message.Date,
                callbackQuery.Message.MessageId,
                "The bot's original prompt",
                domainCategoryEnumCode: domainCategoryEnumCode,
                controlPromptEnumCode: controlPromptEnumCode));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
             callbackQuery, InteractionMode.Operations);
        
        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenUserIsNull_InAnyMode()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
         var basics = GetBasicTestingServices(_services);
         
        var update = new UpdateWrapper(new Message { From = null, Text = "not empty" });
        var conversionResult = await basics.converter.ConvertToModelAsync(update, InteractionMode.Operations);
        Assert.True(conversionResult.IsError);
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenTextAndAttachmentFileIdBothEmpty_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = new UpdateWrapper(new Message { From = new User { Id = 123L } });
        
        var conversionResult = await basics.converter.ConvertToModelAsync(update, InteractionMode.Operations);
        
        Assert.True(conversionResult.IsError);
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenUnsupportedAttachmentTypeLikeAudioSent_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var audioMessage = basics.utils.GetValidTelegramAudioMessage();
        var conversionResult = 
            await basics.converter.ConvertToModelAsync(audioMessage, InteractionMode.Operations);

        Assert.True(conversionResult.IsError);
        Assert.Equal("Failed to convert your Telegram Message: Attachment type Audio is not yet supported!",
            conversionResult.Error!.GetFormattedEnglish());
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
