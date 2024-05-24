using CheckMade.Common.Utils;
using CheckMade.Telegram.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Tests;

internal interface ITestUtils
{
    Randomizer Randomizer { get; }
    
    InputMessage GetValidModelInputTextMessageNoAttachment();
    InputMessage GetValidModelInputTextMessageNoAttachment(long userId);
    InputMessage GetValidModelInputTextMessageWithAttachment();
    
    Message GetValidTelegramTextMessage(string inputText);
    Message GetValidTelegramAudioMessage();
    Message GetValidTelegramDocumentMessage();
    Message GetValidTelegramPhotoMessage();
    Message GetValidTelegramVideoMessage();
    Message GetValidTelegramVoiceMessage();

    Message GetBotCommandMessage(string botCommand);
}

internal class TestUtils(Randomizer randomizer) : ITestUtils
{
    internal const long TestUserDanielGorinTelegramId = 215737196L;

    public Randomizer Randomizer { get; } = randomizer;
    
    public InputMessage GetValidModelInputTextMessageNoAttachment() =>
        GetValidModelInputTextMessageNoAttachment(Randomizer.GenerateRandomLong());

    public InputMessage GetValidModelInputTextMessageNoAttachment(long userId) =>
        new(userId,
            Randomizer.GenerateRandomLong(),
            new MessageDetails(
                DateTime.Now,
                BotType.Submissions,
                $"Hello World, without attachment: {Randomizer.GenerateRandomLong()}",
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None()));
    
    public InputMessage GetValidModelInputTextMessageWithAttachment() =>
        new(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            new MessageDetails(
                DateTime.Now,
                BotType.Submissions,
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                "fakeAttachmentUrl",
                AttachmentType.Photo,
                Option<int>.None()));

    public Message GetValidTelegramTextMessage(string inputText) => 
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Text = inputText
        };

    public Message GetValidTelegramAudioMessage() => 
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeAudioCaption",
            Audio = new Audio { FileId = "fakeAudioFileId" }
        };
    
    public Message GetValidTelegramDocumentMessage() => 
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeDocumentCaption",
            Document = new Document { FileId = "fakeOtherDocumentFileId" }
        };

    public Message GetValidTelegramPhotoMessage() => 
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakePhotoCaption",
            Photo = [new PhotoSize{ Height = 1, Width = 1, FileSize = 100L, FileId = "fakePhotoFileId" }]
        };
    
    public Message GetValidTelegramVideoMessage() =>
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeVideoCaption",
            Video = new Video { FileId = "fakeVideoFileId" }
        };

    public Message GetValidTelegramVoiceMessage() =>
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeVoiceCaption",
            Voice = new Voice { FileId = "fakeVoiceFileId" }
        };

    public Message GetBotCommandMessage(string botCommand) =>
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Text = botCommand,
            Entities = [
                new MessageEntity
                {
                    Length = botCommand.Length,
                    Offset = 0,
                    Type = MessageEntityType.BotCommand
                }
            ]
        };
}