using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.Generic;
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
    
    Message GetValidTelegramTextMessage(string inputText);
    Message GetValidTelegramBotCommandMessage(string botCommand);
    
    Message GetValidTelegramAudioMessage();
    Message GetValidTelegramDocumentMessage();
    Message GetValidTelegramPhotoMessage();
    Message GetValidTelegramVideoMessage();
    Message GetValidTelegramVoiceMessage();
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
            new InputMessageDetails(
                DateTime.Now,
                $"Hello World, without attachment: {Randomizer.GenerateRandomLong()}",
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None()));
    
    public InputMessageDto GetValidModelInputTextMessageWithAttachment(AttachmentType type) =>
        new(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            BotType.Submissions,
            new InputMessageDetails(
                DateTime.Now,
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                "fakeAttachmentUrl",
                type,
                Option<int>.None()));

    public InputMessageDto GetValidModelInputCommandMessage(BotType botType, int botCommandEnumCode) =>
        new(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            botType,
            new InputMessageDetails(
                DateTime.Now,
                Option<string>.None(), 
                Option<string>.None(), 
                Option<AttachmentType>.None(), 
                botCommandEnumCode));

    public Message GetValidTelegramTextMessage(string inputText) => 
        new()
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Text = inputText
        };

    public Message GetValidTelegramBotCommandMessage(string botCommand) =>
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
}