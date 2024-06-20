using CheckMade.Common.Model.Core;
using static CheckMade.Common.Model.Core.RoleType;
using CheckMade.Common.Utils.Generic;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Structs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramUser = Telegram.Bot.Types.User;
using User = CheckMade.Common.Model.Core.User;

namespace CheckMade.Tests;

internal interface ITestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";

    // Needs to be 'long' instead of 'TlgUserId' for usage in InlineData() of Tests - but they implicitly convert
    internal const long TestUserDanielGorinTelegramId = 215737196L;
    internal const long TestUserId_01 = 101L;
    internal const long TestUserId_02 = 102L;
    internal const long TestUserId_03 = 103L;
    
    internal const long TestChatId_01 = 100001L;
    internal const long TestChatId_02 = 100002L;
    internal const long TestChatId_03 = 100003L;
    internal const long TestChatId_04 = 100004L;
    internal const long TestChatId_05 = 100005L;
    internal const long TestChatId_06 = 100006L;
    internal const long TestChatId_07 = 100007L;
    internal const long TestChatId_08 = 100008L;
    internal const long TestChatId_09 = 100009L;
    
    internal static readonly User IntegrationTestsUser = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "IntegrationTest",
        "_Gorin",
        new EmailAddress("daniel-integrtest-checkmade@neocortek.net"),
        LanguageCode.en);

    internal static readonly Role IntegrationTestsRole = new(
        "RAAAA1",
        SanitaryOps_Inspector,
        IntegrationTestsUser);
    
    internal static readonly User UnitTestsUser = new(
        new MobileNumber("+447538521999"),
        "_Daniel",
        "UnitTest",
        "_Gorin",
        Option<EmailAddress>.None(),
        LanguageCode.en);
    
    internal static readonly Role SanitaryOpsAdmin1 = new("VB70TX", SanitaryOps_Admin, UnitTestsUser);
    internal static readonly Role SanitaryOpsInspector1 = new("3UDXWX", SanitaryOps_Inspector, UnitTestsUser);
    internal static readonly Role SanitaryOpsEngineer1 = new("3UED8X", SanitaryOps_Engineer, UnitTestsUser);
    internal static readonly Role SanitaryOpsCleanLead1 = new("2JXNMX", SanitaryOps_CleanLead, UnitTestsUser);
    internal static readonly Role SanitaryOpsObserver1 = new("YEATFX", SanitaryOps_Observer, UnitTestsUser);
    internal static readonly Role SanitaryOpsInspector2 = new("MAM8SX", SanitaryOps_Inspector, UnitTestsUser);
    internal static readonly Role SanitaryOpsEngineer2 = new("P4XPKX", SanitaryOps_Engineer, UnitTestsUser);
    internal static readonly Role SanitaryOpsCleanLead2 = new("I8MJ1X", SanitaryOps_CleanLead, UnitTestsUser);
    internal static readonly Role SanitaryOpsObserver2 = new("67CMCX", SanitaryOps_Observer, UnitTestsUser);
    
    Randomizer Randomizer { get; }
    
    TlgInput GetValidTlgInputTextMessage(long userId = TestUserId_01, long chatId = TestChatId_01, 
        string text = "Hello World", DateTime? dateTime = null);
    TlgInput GetValidTlgInputTextMessageWithAttachment(TlgAttachmentType type);
    TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, long userId = TestUserId_01, long chatId = TestChatId_01);
    TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm, long userId = TestUserId_01, long chatId = TestChatId_01, DateTime? dateTime = null);
    TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts, long userId = TestUserId_01, long chatId = TestChatId_01, DateTime? dateTime = null);
    
    UpdateWrapper GetValidTelegramTextMessage(string inputText, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramBotCommandMessage(string botCommand, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(string callbackQueryData, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramAudioMessage(long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramDocumentMessage(long chatId = TestChatId_01, string fileId = "fakeOtherDocumentFileId");
    UpdateWrapper GetValidTelegramLocationMessage(Option<float> horizontalAccuracy, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramPhotoMessage(long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramVoiceMessage(long chatId = TestChatId_01);

    internal static string GetFirstRawEnglish(Result<IReadOnlyList<OutputDto>> actualOutput)
    {
        var text = actualOutput.GetValueOrThrow()[0].Text.GetValueOrThrow();

        return text.Concatenations.Count > 0
            ? text.Concatenations.First()!.RawEnglishText
            : text.RawEnglishText;
    }
}

internal class TestUtils(Randomizer randomizer) : ITestUtils
{
    public Randomizer Randomizer { get; } = randomizer;
    
    public TlgInput GetValidTlgInputTextMessage(long userId, long chatId, string text, DateTime? dateTime) =>
        new(new TlgAgent(userId, chatId, InteractionMode.Operations),
            TlgInputType.TextMessage,
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow, 
                1, 
                text));
    
    public TlgInput GetValidTlgInputTextMessageWithAttachment(TlgAttachmentType type) =>
        new(new TlgAgent(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            InteractionMode.Operations),
            TlgInputType.AttachmentMessage,
            CreateFromRelevantDetails(
                DateTime.UtcNow,
                1,
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                new Uri("fakeTelegramUri"),
                new Uri("fakeInternalUri"),
                type));

    public TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, long userId, long chatId) =>
        new(new TlgAgent(userId, chatId, interactionMode),
            TlgInputType.CommandMessage,
            CreateFromRelevantDetails(
                DateTime.UtcNow,
                1,
                botCommandEnumCode: botCommandEnumCode));

    public TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm,
        long userId,
        long chatId,
        DateTime? dateTime) =>
        new(new TlgAgent(userId, chatId, InteractionMode.Operations),
            TlgInputType.CallbackQuery,
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow,
                1,
                domainTerm: domainTerm));


    public TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts,
        long userId,
        long chatId,
        DateTime? dateTime) =>
        new(new TlgAgent(userId, chatId, InteractionMode.Operations),
            TlgInputType.CallbackQuery,
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow,
                1,
                controlPromptEnumCode: (long)prompts));

    internal static TlgInputDetails CreateFromRelevantDetails(
        DateTime tlgDate,
        int tlgMessageId,
        string? text = null,
        Uri? attachmentTlgUri = null,
        Uri? attachmentInternalUri = null,
        TlgAttachmentType? attachmentType = null,
        Geo? geoCoordinates = null,
        int? botCommandEnumCode = null,
        DomainTerm? domainTerm = null,
        long? controlPromptEnumCode = null)
    {
        return new TlgInputDetails(
            tlgDate, 
            tlgMessageId,
            text ?? Option<string>.None(),
            attachmentTlgUri ?? Option<Uri>.None(),
            attachmentInternalUri ?? Option<Uri>.None(), 
            attachmentType ?? Option<TlgAttachmentType>.None(),
            geoCoordinates ?? Option<Geo>.None(),
            botCommandEnumCode ?? Option<int>.None(),
            domainTerm ?? Option<DomainTerm>.None(),
            controlPromptEnumCode ?? Option<long>.None());
    }
    
    public UpdateWrapper GetValidTelegramTextMessage(string inputText, long chatId) => 
        new(new Message 
            {
                From = new TelegramUser { Id = Randomizer.GenerateRandomLong() },
                Chat = new Chat { Id = chatId },
                Date = DateTime.UtcNow,
                MessageId = 123,
                Text = inputText
            });

    public UpdateWrapper GetValidTelegramBotCommandMessage(string botCommand, long chatId) =>
        new(new Message
        {
            From = new TelegramUser { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Text = botCommand,
            Entities = [
                new MessageEntity
                {
                    Length = botCommand.Length,
                    Offset = 0,
                    Type = MessageEntityType.BotCommand
                }
            ]
        });

    public UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(
        string callbackQueryData, long chatId) =>
        new(new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Data = callbackQueryData,
                From = new TelegramUser { Id = Randomizer.GenerateRandomLong() }, // The User
                Message = new Message
                {
                    From = new TelegramUser { Id = Randomizer.GenerateRandomLong() }, // The Bot
                    Text = "The bot's original prompt",
                    Date = DateTime.UtcNow,
                    Chat = new Chat { Id = chatId },
                    MessageId = 123,
                }
            }
        });

    public UpdateWrapper GetValidTelegramAudioMessage(long chatId) => 
        new(new Message
        {
            From = new TelegramUser { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakeAudioCaption",
            Audio = new Audio { FileId = "fakeAudioFileId" }
        });

    public UpdateWrapper GetValidTelegramDocumentMessage(long chatId, string fileId) => 
        new(new Message
        {
            From = new TelegramUser { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakeDocumentCaption",
            Document = new Document { FileId = fileId }
        });

    public UpdateWrapper GetValidTelegramLocationMessage(
        Option<float> horizontalAccuracy, long chatId) =>
        new(new Message
        {
            From = new TelegramUser { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Location = new Location
            {
                Latitude = 20.0123,
                Longitude = -17.4509,
                HorizontalAccuracy = horizontalAccuracy.IsSome 
                    ? horizontalAccuracy.GetValueOrThrow() 
                    : null
            }
        });

    public UpdateWrapper GetValidTelegramPhotoMessage(long chatId) => 
        new(new Message
        {
            From = new TelegramUser { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakePhotoCaption",
            Photo = [new PhotoSize{ Height = 1, Width = 1, FileSize = 100L, FileId = "fakePhotoFileId" }]
        });

    public UpdateWrapper GetValidTelegramVoiceMessage(long chatId) =>
        new(new Message
        {
            From = new TelegramUser { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakeVoiceCaption",
            Voice = new Voice { FileId = "fakeVoiceFileId" }
        });
}