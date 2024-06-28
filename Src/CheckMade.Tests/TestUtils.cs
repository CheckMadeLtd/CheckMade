using CheckMade.Common.Utils.Generic;
using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot.Output;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramUser = Telegram.Bot.Types.User;

namespace CheckMade.Tests;

internal interface ITestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";
    
    Randomizer Randomizer { get; }
    
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

