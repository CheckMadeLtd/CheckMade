using CheckMade.Common.Interfaces.Utils;
using CheckMade.Telegram.Model;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests;

internal interface ITestUtils
{
    InputMessage GetValidModelInputTextMessage();
    Message GetValidTextMessage(string inputText);
    Message GetValidAudioMessage();
    Message GetValidDocumentMessage();
    Message GetValidPhotoMessage();
    Message GetValidVideoMessage();
}

internal class TestUtils(IRandomizer randomizer) : ITestUtils
{
    internal const long TestUserDanielGorinTelegramId = 215737196L;
    
    public InputMessage GetValidModelInputTextMessage() =>
        new(randomizer.GenerateRandomLong(),
            new MessageDetails(
                DateTime.Now,
                $"Hello World, Valid Test: {randomizer.GenerateRandomLong()}"
                ));

    public Message GetValidTextMessage(string inputText) => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Text = inputText
        };

    public Message GetValidAudioMessage() => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeAudioCaption",
            Audio = new Audio { FileId = "fakeAudioFileId" }
        };
    
    public Message GetValidDocumentMessage() => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeDocumentCaption",
            Document = new Document { FileId = "fakeOtherDocumentFileId" }
        };

    public Message GetValidPhotoMessage() => 
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakePhotoCaption",
            Photo = [new PhotoSize{ Height = 1, Width = 1, FileSize = 100L, FileId = "fakePhotoFileId" }]
        };
    
    public Message GetValidVideoMessage() =>
        new()
        {
            From = new User { Id = randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            Caption = "fakeVideoCaption",
            Video = new Video { FileId = "fakeVideoFileId" }
        };
}