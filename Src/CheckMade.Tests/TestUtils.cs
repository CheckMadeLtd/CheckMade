using CheckMade.Common.Model.Core;
using CheckMade.Common.Utils.Generic;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramUser = Telegram.Bot.Types.User;
using static CheckMade.Tests.TestData;

namespace CheckMade.Tests;

internal interface ITestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";
    
    Randomizer Randomizer { get; }
    
    TlgInput GetValidTlgInputTextMessage(
        long userId = TestUserId_01, long chatId = TestChatId_01, 
        string text = "Hello World", DateTime? dateTime = null);
    
    TlgInput GetValidTlgInputTextMessageWithAttachment(TlgAttachmentType type);
    
    TlgInput GetValidTlgInputLocationMessage(
        double latitudeRaw, double longitudeRaw, Option<float> uncertaintyRadius, 
        long userId = TestUserId_01, long chatId = TestChatId_01);
    
    TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, 
        long userId = TestUserId_01, long chatId = TestChatId_01,
        int messageId = 1);
    
    TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm, 
        long userId = TestUserId_01, long chatId = TestChatId_01, 
        DateTime? dateTime = null, int messageId = 1);
    
    TlgInput GetValidTlgInputCallbackQueryForControlPrompts(
        ControlPrompts prompts, 
        long userId = TestUserId_01, long chatId = TestChatId_01, DateTime? dateTime = null);
    
    UpdateWrapper GetValidTelegramTextMessage(string inputText, long userId = TestUserId_01, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramBotCommandMessage(string botCommand, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(string callbackQueryData, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramAudioMessage(long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramDocumentMessage(long chatId = TestChatId_01, string fileId = "fakeOtherDocumentFileId");
    UpdateWrapper GetValidTelegramLocationMessage(Option<float> horizontalAccuracy, long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramPhotoMessage(long chatId = TestChatId_01);
    UpdateWrapper GetValidTelegramVoiceMessage(long chatId = TestChatId_01);

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
                new Uri("https://www.gorin.de/fakeTelegramUri1.html"),
                new Uri("https://www.gorin.de/fakeInternalUri1.html"),
                type));

    public TlgInput GetValidTlgInputLocationMessage(
        double latitudeRaw, double longitudeRaw, Option<float> uncertaintyRadius,
        long userId, long chatId) =>
        new(new TlgAgent(userId, chatId, InteractionMode.Operations),
                TlgInputType.Location,
                CreateFromRelevantDetails(
                    DateTime.UtcNow, 
                    1,
                    geoCoordinates: new Geo(latitudeRaw, longitudeRaw, uncertaintyRadius)));

    public TlgInput GetValidTlgInputCommandMessage(
        InteractionMode interactionMode, int botCommandEnumCode, 
        long userId, long chatId, int messageId) =>
        new(new TlgAgent(userId, chatId, interactionMode),
            TlgInputType.CommandMessage,
            CreateFromRelevantDetails(
                DateTime.UtcNow,
                messageId,
                botCommandEnumCode: botCommandEnumCode));

    public TlgInput GetValidTlgInputCallbackQueryForDomainTerm(
        DomainTerm domainTerm,
        long userId, long chatId, DateTime? dateTime, int messageId) =>
        new(new TlgAgent(userId, chatId, InteractionMode.Operations),
            TlgInputType.CallbackQuery,
            CreateFromRelevantDetails(
                dateTime ?? DateTime.UtcNow,
                messageId,
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