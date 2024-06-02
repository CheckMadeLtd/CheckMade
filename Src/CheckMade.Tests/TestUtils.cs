using CheckMade.Common.LangExt;
using CheckMade.Common.Model;
using CheckMade.Common.Utils.Generic;
using CheckMade.Telegram.Function.Services.UpdateHandling;
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
    InputMessageDto GetValidModelInputTextMessage(TelegramUserId userId);
    InputMessageDto GetValidModelInputTextMessageWithAttachment(AttachmentType type);
    InputMessageDto GetValidModelInputCommandMessage(BotType botType, int botCommandEnumCode);
    
    UpdateWrapper GetValidTelegramTextMessage(string inputText);
    UpdateWrapper GetValidTelegramBotCommandMessage(string botCommand);
    UpdateWrapper GetValidTelegramUpdateWithCallbackQuery(string callbackQueryData);
    UpdateWrapper GetValidTelegramAudioMessage();
    UpdateWrapper GetValidTelegramDocumentMessage();
    UpdateWrapper GetValidTelegramLocationMessage(Option<float> horizontalAccuracy);
    UpdateWrapper GetValidTelegramPhotoMessage();
    UpdateWrapper GetValidTelegramVoiceMessage();
}

internal class TestUtils(Randomizer randomizer) : ITestUtils
{
    // Needs to be 'long' instead of 'UserId' for usage in InlineData() of Tests
    internal const long TestUserDanielGorinTelegramId = 215737196L;

    public Randomizer Randomizer { get; } = randomizer;
    
    public InputMessageDto GetValidModelInputTextMessage() =>
        GetValidModelInputTextMessage(Randomizer.GenerateRandomLong());

    public InputMessageDto GetValidModelInputTextMessage(TelegramUserId userId) =>
        new(userId,
            Randomizer.GenerateRandomLong(),
            BotType.Operations,
            ModelUpdateType.TextMessage,
            CreateFromRelevantDetails(
                DateTime.Now,
                1,
                $"Hello World, without attachment: {Randomizer.GenerateRandomLong()}"));
    
    public InputMessageDto GetValidModelInputTextMessageWithAttachment(AttachmentType type) =>
        new(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            BotType.Operations,
            ModelUpdateType.AttachmentMessage,
            CreateFromRelevantDetails(
                DateTime.Now,
                1,
                $"Hello World, with attachment: {Randomizer.GenerateRandomLong()}",
                "fakeAttachmentUrl",
                type));

    public InputMessageDto GetValidModelInputCommandMessage(BotType botType, int botCommandEnumCode) =>
        new(Randomizer.GenerateRandomLong(),
            Randomizer.GenerateRandomLong(),
            botType,
            ModelUpdateType.CommandMessage,
            CreateFromRelevantDetails(
                DateTime.Now,
                1,
                botCommandEnumCode: botCommandEnumCode));

    internal static InputMessageDetails CreateFromRelevantDetails(
        DateTime telegramDate,
        int telegramMessageId,
        string? text = null,
        string? attachmentExternalUrl = null,
        AttachmentType? attachmentType = null,
        Geo? geoCoordinates = null,
        int? botCommandEnumCode = null,
        int? domainCategoryEnumCode = null,
        long? controlPromptEnumCode = null)
    {
        return new InputMessageDetails(
            telegramDate, 
            telegramMessageId,
            text ?? Option<string>.None(),
            attachmentExternalUrl ?? Option<string>.None(),
            attachmentType ?? Option<AttachmentType>.None(),
            geoCoordinates ?? Option<Geo>.None(),
            botCommandEnumCode ?? Option<int>.None(),
            domainCategoryEnumCode ?? Option<int>.None(),
            controlPromptEnumCode ?? Option<long>.None());
    }
    
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

    public UpdateWrapper GetValidTelegramLocationMessage(Option<float> horizontalAccuracy) =>
        new(new Message
        {
            From = new User { Id = Randomizer.GenerateRandomLong() },
            Chat = new Chat { Id = Randomizer.GenerateRandomLong() },
            Date = DateTime.Now,
            MessageId = 123,
            Location = new Location
            {
                Latitude = 20.0123,
                Longitude = -17.4509,
                HorizontalAccuracy = horizontalAccuracy.IsSome 
                    ? horizontalAccuracy.GetValueOrDefault() 
                    : null
            }
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