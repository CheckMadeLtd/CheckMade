using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands.DefinitionsByBot;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;
using static CheckMade.Common.Model.ChatBot.UserInteraction.InteractionMode;
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
        var update = basics.utils.GetValidTelegramTextMessage(textInput);

        var expectedTlgInput = new TlgInput(
            new TlgClientPort(
                update.Message.From!.Id,
                update.Message.Chat.Id,
                Operations),
            TlgInputType.TextMessage,
            TestUtils.CreateFromRelevantDetails(
                update.Message.Date,
                update.Message.MessageId,
                update.Message.Text));

        var actualTlgInput = 
            await basics.converter.ConvertToModelAsync(update, Operations);

        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());
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
            TlgAttachmentType.Document => basics.utils.GetValidTelegramDocumentMessage(),
            TlgAttachmentType.Photo => basics.utils.GetValidTelegramPhotoMessage(),
            TlgAttachmentType.Voice => basics.utils.GetValidTelegramVoiceMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var expectedAttachmentTlgUri = new Uri(
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{basics.mockBotClient.Object.MyBotToken}/" +
            $"{(await basics.mockBotClient.Object.GetFileAsync("any")).FilePath}");

        var expectedTlgInput = new TlgInput(
            new TlgClientPort(
                attachmentUpdate.Message.From!.Id,
                attachmentUpdate.Message.Chat.Id,
                Operations),
            TlgInputType.AttachmentMessage,
            TestUtils.CreateFromRelevantDetails(
                attachmentUpdate.Message.Date,
                attachmentUpdate.Message.MessageId,
                attachmentUpdate.Message.Caption,
                expectedAttachmentTlgUri,
                new Uri("https://gorin.de/Can_test_for_this_only_in_integration_tests"),
                attachmentType));
        
        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            attachmentUpdate, Operations);
        
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
            new TlgClientPort(
                locationUpdate.Message.From!.Id, 
                locationUpdate.Message.Chat.Id, 
                Operations),
                TlgInputType.Location,
                TestUtils.CreateFromRelevantDetails(
                    locationUpdate.Message.Date,
                    locationUpdate.Message.MessageId,
                    geoCoordinates: expectedGeoCoordinates));
        
        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            locationUpdate, Operations);
        
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
            new TlgClientPort(
                commandUpdate.Message.From!.Id,
                commandUpdate.Message.Chat.Id,
                Operations),
            TlgInputType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            commandUpdate, Operations);
        
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
            new TlgClientPort(
                commandUpdate.Message.From!.Id,
                commandUpdate.Message.Chat.Id,
                Communications),
            TlgInputType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            commandUpdate, Communications);
        
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
            new TlgClientPort(
                commandUpdate.Message.From!.Id,
                commandUpdate.Message.Chat.Id,
                Notifications),
            TlgInputType.CommandMessage,
            TestUtils.CreateFromRelevantDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                commandText,
                botCommandEnumCode: (int)command));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
            commandUpdate, Notifications);
        
        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());        
    }

    [Theory]
    [InlineData((long)ControlPrompts.Good)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForMessageWithCallbackQuery_InAnyMode(
        long enumSourceOfCallbackQuery)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var callbackQueryData = new CallbackId(enumSourceOfCallbackQuery);
        var callbackQuery = basics.utils.GetValidTelegramUpdateWithCallbackQuery(callbackQueryData);
        var controlPromptEnumCode = (long?)long.Parse(callbackQuery.Update.CallbackQuery!.Data!);

        var expectedTlgInput = new TlgInput(
            new TlgClientPort(
                callbackQuery.Message.From!.Id,
                callbackQuery.Message.Chat.Id,
                Operations),
            TlgInputType.CallbackQuery,
            TestUtils.CreateFromRelevantDetails(
                callbackQuery.Message.Date,
                callbackQuery.Message.MessageId,
                "The bot's original prompt",
                controlPromptEnumCode: controlPromptEnumCode));

        var actualTlgInput = await basics.converter.ConvertToModelAsync(
             callbackQuery, Operations);
        
        Assert.Equivalent(expectedTlgInput, actualTlgInput.GetValueOrThrow());
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
        
        var conversionResult = await basics.converter.ConvertToModelAsync(update, Operations);
        
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
        
        var conversionResult = await basics.converter.ConvertToModelAsync(update, Operations);
        
        Assert.True(conversionResult.IsError);
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsError_WhenUnsupportedAttachmentTypeLikeAudioSent_InAnyMode()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var audioMessage = basics.utils.GetValidTelegramAudioMessage();
        var conversionResult = 
            await basics.converter.ConvertToModelAsync(audioMessage, Operations);

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
