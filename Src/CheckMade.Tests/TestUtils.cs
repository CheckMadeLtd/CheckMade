using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.Generic;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.DTOs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Tests;

internal interface ITestUtils
{
    // These string values to be exactly the same as in the corresponding .tsv translation files! 
    internal static readonly UiString EnglishUiStringForTests = Ui("English string for testing");
    internal const string GermanStringForTests = "Deutscher Text für Tests";
    
    Randomizer Randomizer { get; }
    
    InputMessageDto GetValidModelInputTextMessage();
    InputMessageDto GetValidModelInputTextMessage(long userId);
    InputMessageDto GetValidModelInputTextMessageWithAttachment(AttachmentType type);
    InputMessageDto GetValidModelInputCommandMessage(BotType botType, int botCommandEnumCode);
    
    UpdateWrapper GetValidTelegramTextMessage(string inputText);
    UpdateWrapper GetValidTelegramBotCommandMessage(string botCommand);
    
    UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(string callbackQueryData);
    
    UpdateWrapper GetValidTelegramAudioMessage();
    UpdateWrapper GetValidTelegramDocumentMessage();
    UpdateWrapper GetValidTelegramPhotoMessage();
    UpdateWrapper GetValidTelegramVoiceMessage();
}

internal class TestUtils(Randomizer randomizer) : ITestUtils
{
    internal const long TestUserDanielGorinTelegramId = 215737196L;

    public Randomizer Randomizer { get; } = randomizer;
    
    public InputMessageDto GetValidModelInputTextMessage() =>
        GetValidModelInputTextMessage(Randomizer.GenerateRandomLong());

    public InputMessageDto GetValidModelInputTextMessage(long userId) =>
        new(userId,
            Randomizer.GenerateRandomLong(),
            BotType.Submissions,
            ModelUpdateType.TextMessage,
            new InputMessageDetails(
                DateTime.Now,
                1,
                $"Hello World, without attachment: {Randomizer.GenerateRandomLong()}",
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None(),
                Option<int>.None(),
                Option<long>.None()));
    
    public InputMessageDto GetValidModelInputTextMessageWithAttachment(AttachmentType type) =>
        new(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            BotType.Submissions,
            ModelUpdateType.AttachmentMessage,
            new InputMessageDetails(
                DateTime.Now,
                1,
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                "fakeAttachmentUrl",
                type,
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));

    public InputMessageDto GetValidModelInputCommandMessage(BotType botType, int botCommandEnumCode) =>
        new(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            botType,
            ModelUpdateType.CommandMessage,
            new InputMessageDetails(
                DateTime.Now,
                1,
                Option<string>.None(), 
                Option<string>.None(), 
                Option<AttachmentType>.None(), 
                botCommandEnumCode,
                Option<int>.None(), 
                Option<long>.None()));

    public UpdateWrapper GetValidTelegramTextMessage(string inputText) => 
        new(new Message 
            {
                From = new User { Id = Randomizer.GenerateRandomLong() },
                Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
                Date = DateTime.Now,
                MessageId = 123,
                Text = inputText
            });

    public UpdateWrapper GetValidTelegramBotCommandMessage(string botCommand) =>
        new(new Message
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
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

    public UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(string callbackQueryData) =>
        new(new Update
        {
            CallbackQuery = new CallbackQuery
            {
                Data = callbackQueryData,
                Message = new Message
                {
                    From = new User { Id = Randomizer.GenerateRandomLong() },
                    Text = "The bot's original prompt",
                    Date = DateTime.Now,
                    Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
                    MessageId = 123,
                }
            }
        });

    public UpdateWrapper GetValidTelegramAudioMessage() => 
        new(new Message
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            MessageId = 123,
            Caption = "fakeAudioCaption",
            Audio = new Audio { FileId = "fakeAudioFileId" }
        });

    public UpdateWrapper GetValidTelegramDocumentMessage() => 
        new(new Message
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            MessageId = 123,
            Caption = "fakeDocumentCaption",
            Document = new Document { FileId = "fakeOtherDocumentFileId" }
        });

    public UpdateWrapper GetValidTelegramPhotoMessage() => 
        new(new Message
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            MessageId = 123,
            Caption = "fakePhotoCaption",
            Photo = [new PhotoSize{ Height = 1, Width = 1, FileSize = 100L, FileId = "fakePhotoFileId" }]
        });

    public UpdateWrapper GetValidTelegramVoiceMessage() =>
        new(new Message
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            MessageId = 123,
            Caption = "fakeVoiceCaption",
            Voice = new Voice { FileId = "fakeVoiceFileId" }
        });
}