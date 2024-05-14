using CheckMade.Common.Interfaces.Utils;
using CheckMade.Common.LanguageExtensions.MonadicWrappers;
using CheckMade.Telegram.Model;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests;

internal interface ITestUtils
{
    InputMessage GetValidModelInputTextMessage();
    Message GetValidTelegramTextMessage(string inputText);
    Message GetValidTelegramAudioMessage();
    Message GetValidTelegramDocumentMessage();
    Message GetValidTelegramPhotoMessage();
    Message GetValidTelegramVideoMessage();
}

internal class TestUtils(IRandomizer randomizer) : ITestUtils
{
    internal const long TestUserDanielGorinTelegramId = 215737196L;
    
    public InputMessage GetValidModelInputTextMessage() =>
        new(randomizer.GenerateRandomLong(),
            randomizer.GenerateRandomLong(),
            new MessageDetails(
                DateTime.Now,
                $"Hello World, Valid Test: {randomizer.GenerateRandomLong()}",
                Option<string>.None(),
                Option<AttachmentType>.None()));

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
}