using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Utils.Generic;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramUser = Telegram.Bot.Types.User;

namespace CheckMade.Tests.Utils;

internal interface ITelegramUpdateGenerator
{
    Randomizer Randomizer { get; }
    
    UpdateWrapper GetValidTelegramTextMessage(
        string inputText, 
        long userId = Default_UserAndChatId_PrivateBotChat, 
        long chatId = Default_UserAndChatId_PrivateBotChat);
    
    UpdateWrapper GetValidTelegramBotCommandMessage(
        string botCommand, 
        long chatId = Default_UserAndChatId_PrivateBotChat);
    
    UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(
        string callbackQueryData,
        long chatId = Default_UserAndChatId_PrivateBotChat);
    
    UpdateWrapper GetValidTelegramAudioMessage(
        long chatId = Default_UserAndChatId_PrivateBotChat);
    
    UpdateWrapper GetValidTelegramDocumentMessage(
        long chatId = Default_UserAndChatId_PrivateBotChat, 
        string fileId = "fakeOtherDocumentFileId");
    
    UpdateWrapper GetValidTelegramLocationMessage(
        Option<float> horizontalAccuracy,
        long chatId = Default_UserAndChatId_PrivateBotChat);
    
    UpdateWrapper GetValidTelegramPhotoMessage(
        long chatId = Default_UserAndChatId_PrivateBotChat);
    
    UpdateWrapper GetValidTelegramVoiceMessage(
        long chatId = Default_UserAndChatId_PrivateBotChat);
}

internal class TelegramUpdateGenerator(Randomizer randomizer) : ITelegramUpdateGenerator
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
            From = new TelegramUser { Id = Default_UserAndChatId_PrivateBotChat },
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
                From = new TelegramUser { Id = Default_UserAndChatId_PrivateBotChat }, // The User
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
            From = new TelegramUser { Id = Default_UserAndChatId_PrivateBotChat },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakeAudioCaption",
            Audio = new Audio { FileId = "fakeAudioFileId" }
        });

    public UpdateWrapper GetValidTelegramDocumentMessage(long chatId, string fileId) => 
        new(new Message
        {
            From = new TelegramUser { Id = Default_UserAndChatId_PrivateBotChat },
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
            From = new TelegramUser { Id = Default_UserAndChatId_PrivateBotChat },
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
            From = new TelegramUser { Id = Default_UserAndChatId_PrivateBotChat },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakePhotoCaption",
            Photo = [new PhotoSize{ Height = 1, Width = 1, FileSize = 100L, FileId = "fakePhotoFileId" }]
        });

    public UpdateWrapper GetValidTelegramVoiceMessage(long chatId) =>
        new(new Message
        {
            From = new TelegramUser { Id = Default_UserAndChatId_PrivateBotChat },
            Chat = new Chat { Id = chatId },
            Date = DateTime.UtcNow,
            MessageId = 123,
            Caption = "fakeVoiceCaption",
            Voice = new Voice { FileId = "fakeVoiceFileId" }
        });
}

