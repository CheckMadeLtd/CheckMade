using CheckMade.Common.Model.Core;
using CheckMade.Common.Utils.Generic;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramUser = Telegram.Bot.Types.User;
using static CheckMade.Tests.TestOriginatorRoleSetting;

namespace CheckMade.Tests;

internal interface ITestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";
    
    Randomizer Randomizer { get; }
    
    
    TlgInput GetValidTlgInputTextMessage(
        long userId = TestUserAndChatId01_PrivateChat_Default, 
        long chatId = TestUserAndChatId01_PrivateChat_Default, 
        string text = "Hello World", DateTime? dateTime = null,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputTextMessageWithAttachment(
        TlgAttachmentType type,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputLocationMessage(
        double latitudeRaw, double longitudeRaw, Option<float> uncertaintyRadius, 
        long userId = TestUserAndChatId01_PrivateChat_Default, 
        long chatId = TestUserAndChatId01_PrivateChat_Default,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, 
        long userId = TestUserAndChatId01_PrivateChat_Default, 
        long chatId = TestUserAndChatId01_PrivateChat_Default,
        int messageId = 1,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm, 
        long userId = TestUserAndChatId01_PrivateChat_Default, 
        long chatId = TestUserAndChatId01_PrivateChat_Default, 
        DateTime? dateTime = null, int messageId = 1,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts, 
        long userId = TestUserAndChatId01_PrivateChat_Default, 
        long chatId = TestUserAndChatId01_PrivateChat_Default,
        DateTime? dateTime = null,
        TestOriginatorRoleSetting roleSetting = UnitTestDefault);
    
    
    UpdateWrapper GetValidTelegramTextMessage(
        string inputText, 
        long userId = TestUserAndChatId01_PrivateChat_Default, 
        long chatId = TestUserAndChatId01_PrivateChat_Default);
    
    UpdateWrapper GetValidTelegramBotCommandMessage(
        string botCommand, 
        long chatId = TestUserAndChatId01_PrivateChat_Default);
    
    UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(
        string callbackQueryData,
        long chatId = TestUserAndChatId01_PrivateChat_Default);
    
    UpdateWrapper GetValidTelegramAudioMessage(
        long chatId = TestUserAndChatId01_PrivateChat_Default);
    
    UpdateWrapper GetValidTelegramDocumentMessage(
        long chatId = TestUserAndChatId01_PrivateChat_Default, 
        string fileId = "fakeOtherDocumentFileId");
    
    UpdateWrapper GetValidTelegramLocationMessage(
        Option<float> horizontalAccuracy,
        long chatId = TestUserAndChatId01_PrivateChat_Default);
    
    UpdateWrapper GetValidTelegramPhotoMessage(
        long chatId = TestUserAndChatId01_PrivateChat_Default);
    
    UpdateWrapper GetValidTelegramVoiceMessage(
        long chatId = TestUserAndChatId01_PrivateChat_Default);

    
    internal static string GetFirstRawEnglish(Result<IReadOnlyCollection<OutputDto>> actualOutput) => 
        GetFirstRawEnglish(actualOutput.GetValueOrThrow());

    internal static string GetFirstRawEnglish(IReadOnlyCollection<OutputDto> actualOutput)
    {
        var text = actualOutput.First().Text.GetValueOrThrow();

        return text.Concatenations.Count > 0
            ? text.Concatenations.First()!.RawEnglishText
            : text.RawEnglishText;
    }
}

internal class TestUtils(Randomizer randomizer) : ITestUtils
{
    public Randomizer Randomizer { get; } = randomizer;

    public TlgInput GetValidTlgInputTextMessage(
        long userId, long chatId, string text, DateTime? dateTime,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.TextMessage,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow, 
                1, 
                text));
    }

    public TlgInput GetValidTlgInputTextMessageWithAttachment(
        TlgAttachmentType type, 
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(TestUserAndChatId01_PrivateChat_Default,
                TestUserAndChatId01_PrivateChat_Default,
                Operations),
            TlgInputType.AttachmentMessage,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                DateTime.UtcNow,
                1,
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                new Uri("https://www.gorin.de/fakeTelegramUri1.html"),
                new Uri("https://www.gorin.de/fakeInternalUri1.html"),
                type));
    }

    public TlgInput GetValidTlgInputLocationMessage(
        double latitudeRaw, double longitudeRaw, Option<float> uncertaintyRadius,
        long userId, long chatId,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.Location,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                DateTime.UtcNow, 
                1,
                geoCoordinates: new Geo(latitudeRaw, longitudeRaw, uncertaintyRadius)));
    }

    public TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode,
        long userId, long chatId, int messageId,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, interactionMode),
            TlgInputType.CommandMessage,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                DateTime.UtcNow,
                messageId,
                botCommandEnumCode: botCommandEnumCode));
    }

    public TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm,
        long userId, long chatId, DateTime? dateTime, int messageId,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow,
                messageId,
                domainTerm: domainTerm));
    }

    public TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts,
        long userId, long chatId, DateTime? dateTime,
        TestOriginatorRoleSetting roleSetting)
    {
        return new TlgInput(
            new TlgAgent(userId, chatId, Operations),
            TlgInputType.CallbackQuery,
            GetInputContextInfo(roleSetting).originatorRole, 
            GetInputContextInfo(roleSetting).liveEvent, 
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow,
                1,
                controlPromptEnumCode: (long)prompts));
    }

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

    
    private static (Option<IRoleInfo> originatorRole, Option<ILiveEventInfo> liveEvent)
        GetInputContextInfo(TestOriginatorRoleSetting roleSetting)
    {
        return roleSetting switch
        {
            None => 
                (Option<IRoleInfo>.None(),
                    Option<ILiveEventInfo>.None()),
            
            UnitTestDefault =>
                (SanitaryOpsAdmin_AtMockParooka2024_Default,
                    Option<ILiveEventInfo>.Some(SanitaryOpsAdmin_AtMockParooka2024_Default.AtLiveEvent)),
            
            IntegrationTestDefault =>
                (IntegrationTests_Role_Default, 
                    Option<ILiveEventInfo>.Some(IntegrationTests_Role_Default.AtLiveEvent)),
            
            _ => throw new ArgumentOutOfRangeException(nameof(roleSetting))
        };
    }
    
    
    public UpdateWrapper GetValidTelegramTextMessage(string inputText, long userId, long chatId) => 
        new(new Message 
            {
                From = new TelegramUser { Id = userId },
                Chat = new Chat { Id = chatId },
                Date = DateTime.UtcNow,
                MessageId = 123,
                Text = inputText
            });

    public UpdateWrapper GetValidTelegramBotCommandMessage(string botCommand, long chatId) =>
        new(new Message
        {
            From = new TelegramUser { Id = TestUserAndChatId01_PrivateChat_Default },
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
                From = new TelegramUser { Id = TestUserAndChatId01_PrivateChat_Default }, // The User
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
            From = new TelegramUser { Id = TestUserAndChatId01_PrivateChat_Default },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakeAudioCaption",
            Audio = new Audio { FileId = "fakeAudioFileId" }
        });

    public UpdateWrapper GetValidTelegramDocumentMessage(long chatId, string fileId) => 
        new(new Message
        {
            From = new TelegramUser { Id = TestUserAndChatId01_PrivateChat_Default },
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
            From = new TelegramUser { Id = TestUserAndChatId01_PrivateChat_Default },
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
            From = new TelegramUser { Id = TestUserAndChatId01_PrivateChat_Default },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakePhotoCaption",
            Photo = [new PhotoSize{ Height = 1, Width = 1, FileSize = 100L, FileId = "fakePhotoFileId" }]
        });

    public UpdateWrapper GetValidTelegramVoiceMessage(long chatId) =>
        new(new Message
        {
            From = new TelegramUser { Id = TestUserAndChatId01_PrivateChat_Default },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakeVoiceCaption",
            Voice = new Voice { FileId = "fakeVoiceFileId" }
        });
}

internal enum TestOriginatorRoleSetting
{
    UnitTestDefault,
    IntegrationTestDefault,
    None
}
