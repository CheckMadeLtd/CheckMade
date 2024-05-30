using CheckMade.Common.LangExt;
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
            BotType.Submissions,
            new InputMessageDetails(
                update.Message.Date,
                update.Message.MessageId,
                !string.IsNullOrWhiteSpace(update.Message.Text) 
                    ? update.Message.Text 
                    : Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));

        var actualInputMessage = 
            await basics.converter.ConvertToModelAsync(update, BotType.Submissions);

        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }
    
    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    [InlineData(AttachmentType.Video)]
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
            AttachmentType.Video => basics.utils.GetValidTelegramVideoMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var expectedAttachmentExternalUrl =
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{basics.mockBotClient.Object.BotToken}/" +
            $"{(await basics.mockBotClient.Object.GetFileOrThrowAsync("any")).FilePath}";

        var expectedInputMessage = new InputMessageDto(
            attachmentUpdate.Message.From!.Id,
            attachmentUpdate.Message.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                attachmentUpdate.Message.Date,
                attachmentUpdate.Message.MessageId,
                !string.IsNullOrWhiteSpace(attachmentUpdate.Message.Caption)
                    ? attachmentUpdate.Message.Caption
                    : Option<string>.None(),
                expectedAttachmentExternalUrl,
                attachmentType,
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));
        
        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            attachmentUpdate, BotType.Submissions);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }

    [Theory]
    [InlineData(SubmissionsBotCommands.NewIssue)]
    [InlineData(SubmissionsBotCommands.NewAssessment)]
    [InlineData(SubmissionsBotCommands.Settings)]
    [InlineData(SubmissionsBotCommands.Logout)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForBotCommandMessage_ToSubmissions(
        SubmissionsBotCommands command)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var submissionsCommandMenu = 
            new BotCommandMenus().SubmissionsBotCommandMenu;
        var currentCommand = submissionsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(currentCommand);

        var expectedInputMessage = new InputMessageDto(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                currentCommand,
                Option<string>.None(),
                Option<AttachmentType>.None(),
                (int)command,
                Option<int>.None(),
                Option<long>.None()));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            commandUpdate, BotType.Submissions);
        
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
        var currentCommand = communicationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(currentCommand);

        var expectedInputMessage = new InputMessageDto(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Communications,
            new InputMessageDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                currentCommand,
                Option<string>.None(),
                Option<AttachmentType>.None(),
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
        var currentCommand = notificationsCommandMenu[command][LanguageCode.en].Command;
        var commandUpdate = basics.utils.GetValidTelegramBotCommandMessage(currentCommand);

        var expectedInputMessage = new InputMessageDto(
            commandUpdate.Message.From!.Id,
            commandUpdate.Message.Chat.Id,
            BotType.Notifications,
            new InputMessageDetails(
                commandUpdate.Message.Date,
                commandUpdate.Message.MessageId,
                currentCommand,
                Option<string>.None(),
                Option<AttachmentType>.None(),
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

        var domainCategoryEnumCode = enumSourceOfCallbackQuery <= 99999
            ? Option<int>.Some(int.Parse(callbackQuery.Update.CallbackQuery!.Data!))
            : Option<int>.None();

        var controlPromptEnumCode = enumSourceOfCallbackQuery >= 1L<<17
            ? Option<long>.Some(long.Parse(callbackQuery.Update.CallbackQuery!.Data!))
            : Option<long>.None();

        var expectedInputMessage = new InputMessageDto(
            callbackQuery.Update.CallbackQuery!.From.Id,
            callbackQuery.Update.CallbackQuery.Message!.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                callbackQuery.Update.CallbackQuery.Message.Date,
                callbackQuery.Update.CallbackQuery.Message.MessageId,
                Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None(),
                domainCategoryEnumCode,
                controlPromptEnumCode));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
             callbackQuery, BotType.Submissions);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenUserIsNull_ForAnyBotType()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
         var basics = GetBasicTestingServices(_services);
         
        var update = new UpdateWrapper(new Message { From = null, Text = "not empty" });
        var conversionAttempt = await basics.converter.ConvertToModelAsync(update, BotType.Submissions);
        Assert.True(conversionAttempt.IsFailure);
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenTextAndAttachmentFileIdBothEmpty_ForAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var update = new UpdateWrapper(new Message { From = new User { Id = 123L } });
        
        var conversionAttempt = await basics.converter.ConvertToModelAsync(update, BotType.Submissions);
        
        Assert.True(conversionAttempt.IsFailure);
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenUnsupportedAttachmentTypeLikeVoiceSent_ToAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var voiceMessage = basics.utils.GetValidTelegramVoiceMessage();
        var conversionAttempt = await basics.converter.ConvertToModelAsync(voiceMessage, BotType.Submissions);

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
