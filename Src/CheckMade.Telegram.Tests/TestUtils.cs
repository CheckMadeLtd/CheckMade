using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Logic.BotCommandEnums;
using CheckMade.Telegram.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Tests;

internal interface ITestUtils
{
    InputMessage GetValidModelInputTextMessageNoAttachment();
    InputMessage GetValidModelInputTextMessageNoAttachment(long userId);
    InputMessage GetValidModelInputTextMessageWithAttachment();
    
    Message GetValidTelegramTextMessage(string inputText);
    Message GetValidTelegramAudioMessage();
    Message GetValidTelegramDocumentMessage();
    Message GetValidTelegramPhotoMessage();
    Message GetValidTelegramVideoMessage();
    Message GetValidTelegramVoiceMessage();

    Message GetValidSubmissionsBotCommandMessage(SubmissionsBotCommands botCommand);
}

internal class TestUtils(Randomizer randomizer) : ITestUtils
{
    internal const long TestUserDanielGorinTelegramId = 215737196L;
    
    public InputMessage GetValidModelInputTextMessageNoAttachment() =>
        GetValidModelInputTextMessageNoAttachment(randomizer.GenerateRandomLong());

    public InputMessage GetValidModelInputTextMessageNoAttachment(long userId) =>
        new(userId,
            randomizer.GenerateRandomLong(),
            new MessageDetails(
                DateTime.Now,
                $"Hello World, without attachment: {randomizer.GenerateRandomLong()}",
                Option<string>.None(),
                Option<AttachmentType>.None()));
    
    public InputMessage GetValidModelInputTextMessageWithAttachment() =>
        new(randomizer.GenerateRandomLong(),
            randomizer.GenerateRandomLong(),
            new MessageDetails(
                DateTime.Now,
                $"Hello World, with attachment: {randomizer.GenerateRandomLong()}",
                "fakeAttachmentUrl",
                AttachmentType.Photo));

    public Message GetValidTelegramTextMessage(string inputText) => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Text = inputText
        };

    public Message GetValidTelegramAudioMessage() => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeAudioCaption",
            Audio = new Audio { FileId = "fakeAudioFileId" }
        };
    
    public Message GetValidTelegramDocumentMessage() => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeDocumentCaption",
            Document = new Document { FileId = "fakeOtherDocumentFileId" }
        };

    public Message GetValidTelegramPhotoMessage() => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakePhotoCaption",
            Photo = [new PhotoSize{ Height = 1, Width = 1, FileSize = 100L, FileId = "fakePhotoFileId" }]
        };
    
    public Message GetValidTelegramVideoMessage() =>
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeVideoCaption",
            Video = new Video { FileId = "fakeVideoFileId" }
        };

    public Message GetValidTelegramVoiceMessage() =>
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeVoiceCaption",
            Voice = new Voice { FileId = "fakeVoiceFileId" }
        };

    public Message GetValidSubmissionsBotCommandMessage(SubmissionsBotCommands botCommand) =>
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Text = botCommand.ToString(),
            Entities = [
                new MessageEntity
                {
                    Length = botCommand.ToString().Length,
                    Offset = 0,
                    Type = MessageEntityType.BotCommand
                }
            ]
        };
}