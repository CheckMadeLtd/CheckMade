using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Enums;
using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
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
        var telegramInputMessage = basics.utils.GetValidTelegramTextMessage(textInput);

        var expectedInputMessage = new InputMessageDto(
            telegramInputMessage.From!.Id,
            telegramInputMessage.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                telegramInputMessage.Date,
                telegramInputMessage.MessageId,
                !string.IsNullOrWhiteSpace(telegramInputMessage.Text) 
                    ? telegramInputMessage.Text 
                    : Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));

        var actualInputMessage = 
            await basics.converter.ConvertToModelAsync(telegramInputMessage, BotType.Submissions);

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
        var telegramAttachmentMessage = attachmentType switch
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
            telegramAttachmentMessage.From!.Id,
            telegramAttachmentMessage.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                telegramAttachmentMessage.Date,
                telegramAttachmentMessage.MessageId,
                !string.IsNullOrWhiteSpace(telegramAttachmentMessage.Caption)
                    ? telegramAttachmentMessage.Caption
                    : Option<string>.None(),
                expectedAttachmentExternalUrl,
                attachmentType,
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));
        
        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            telegramAttachmentMessage, BotType.Submissions);
        
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
        var telegramCommandMessage = basics.utils.GetValidTelegramBotCommandMessage(currentCommand);

        var expectedInputMessage = new InputMessageDto(
            telegramCommandMessage.From!.Id,
            telegramCommandMessage.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                telegramCommandMessage.Date,
                telegramCommandMessage.MessageId,
                currentCommand,
                Option<string>.None(),
                Option<AttachmentType>.None(),
                (int)command,
                Option<int>.None(),
                Option<long>.None()));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            telegramCommandMessage, BotType.Submissions);
        
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
        var telegramCommandMessage = basics.utils.GetValidTelegramBotCommandMessage(currentCommand);

        var expectedInputMessage = new InputMessageDto(
            telegramCommandMessage.From!.Id,
            telegramCommandMessage.Chat.Id,
            BotType.Communications,
            new InputMessageDetails(
                telegramCommandMessage.Date,
                telegramCommandMessage.MessageId,
                currentCommand,
                Option<string>.None(),
                Option<AttachmentType>.None(),
                (int)command,
                Option<int>.None(),
                Option<long>.None()));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            telegramCommandMessage, BotType.Communications);
        
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
        var telegramCommandMessage = basics.utils.GetValidTelegramBotCommandMessage(currentCommand);

        var expectedInputMessage = new InputMessageDto(
            telegramCommandMessage.From!.Id,
            telegramCommandMessage.Chat.Id,
            BotType.Notifications,
            new InputMessageDetails(
                telegramCommandMessage.Date,
                telegramCommandMessage.MessageId,
                currentCommand,
                Option<string>.None(),
                Option<AttachmentType>.None(),
                (int)command,
                Option<int>.None(),
                Option<long>.None()));

        var actualInputMessage = await basics.converter.ConvertToModelAsync(
            telegramCommandMessage, BotType.Notifications);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());        
    }

    [Theory]
    [InlineData((long)DomainCategory.SanitaryOpsIssueCleanliness)]
    [InlineData((long)ControlPrompts.Good)]
    public async Task ConvertToModelAsync_ConvertsWithCorrectDetails_ForMessageWithCallbackQuery_ToAnyBot(
        long callbackQuerySource)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var callbackQueryData = new EnumCallbackId(callbackQuerySource).Id;
        
        var callbackQuerySourceType = callbackQuerySource switch
        {
            < 1000 => typeof(DomainCategory), // ToDo: Put the correct threshold in
            _ => typeof(ControlPrompts)
        };
        var callbackQueryUpdate = basics.utils.GetValidTelegramUpdateWithCallbackQuery(callbackQueryData);

        var domainCategoryEnumCode = callbackQuerySourceType == typeof(DomainCategory)
            ? Option<int>.Some(Int32.Parse(callbackQueryUpdate.CallbackQuery!.Data!))
            : Option<int>.None();

        var controlPromptEnumCode = callbackQuerySourceType == typeof(ControlPrompts)
            ? Option<long>.Some(Int64.Parse(callbackQueryUpdate.CallbackQuery!.Data!))
            : Option<long>.None();

        var expectedInputMessage = new InputMessageDto(
            callbackQueryUpdate.CallbackQuery!.From.Id,
            callbackQueryUpdate.CallbackQuery.Message!.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                callbackQueryUpdate.CallbackQuery.Message.Date,
                callbackQueryUpdate.CallbackQuery.Message.MessageId,
                Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None(),
                domainCategoryEnumCode,
                controlPromptEnumCode));

        // var actualInputMessage = await basics.converter.ConvertToModelAsync(
        //     callbackQueryUpdate, BotType.Submissions);
        
        // Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }

    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenUserIsNull_ForAnyBotType()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
         var basics = GetBasicTestingServices(_services);
         
        var telegramMessage = new Message { From = null, Text = "not empty" };
        var conversionAttempt = await basics.converter.ConvertToModelAsync(telegramMessage, BotType.Submissions);
        Assert.True(conversionAttempt.IsFailure);
    }
    
    [Fact]
    public async Task ConvertToModelAsync_ReturnsFailure_WhenTextAndAttachmentFileIdBothEmpty_ForAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        var telegramMessage = new Message { From = new User { Id = 123L } };
        
        var conversionAttempt = await basics.converter.ConvertToModelAsync(telegramMessage, BotType.Submissions);
        
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
