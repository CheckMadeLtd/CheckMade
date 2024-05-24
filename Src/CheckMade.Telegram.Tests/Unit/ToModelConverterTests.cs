using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.UiTranslation;
using CheckMade.Telegram.Function.Services;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Tests.Startup;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Tests.Unit;

public class ToModelConverterTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData("Normal valid text message")]
    [InlineData("_")]
    [InlineData(" valid text message \n with line break and trailing spaces ")]
    public async Task ConvertMessage_ConvertsWithCorrectDetails_ForValidTextMessage_ToAnyBotType(
        string textInput)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var telegramInputMessage = utils.GetValidTelegramTextMessage(textInput);
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(
            new TelegramFilePathResolver(mockBotClient.Object),
            _services.GetRequiredService<IUiTranslator>());

        var expectedInputMessage = new InputMessage(
            telegramInputMessage.From!.Id,
            telegramInputMessage.Chat.Id,
            new MessageDetails(
                telegramInputMessage.Date,
                BotType.Submissions,
                !string.IsNullOrWhiteSpace(telegramInputMessage.Text) 
                    ? telegramInputMessage.Text 
                    : Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None()));

        // Act
        var actualInputMessage = await converter.ConvertMessageOrThrowAsync(telegramInputMessage, BotType.Submissions);

        // Assert
        actualInputMessage.Should().BeEquivalentTo(expectedInputMessage);
    }
    
    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    [InlineData(AttachmentType.Video)]
    public async Task ConvertMessage_ConvertsWithCorrectDetails_ForValidAttachmentMessage_ToAnyBotType(
        AttachmentType attachmentType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        var telegramAttachmentMessage = attachmentType switch
        {
            AttachmentType.Audio => utils.GetValidTelegramAudioMessage(),
            AttachmentType.Document => utils.GetValidTelegramDocumentMessage(),
            AttachmentType.Photo => utils.GetValidTelegramPhotoMessage(),
            AttachmentType.Video => utils.GetValidTelegramVideoMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(
            new TelegramFilePathResolver(mockBotClient.Object),
            _services.GetRequiredService<IUiTranslator>());

        var expectedAttachmentExternalUrl =
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{mockBotClient.Object.BotToken}/" +
            $"{(await mockBotClient.Object.GetFileOrThrowAsync("any")).FilePath}";

        var expectedInputMessage = new InputMessage(
            telegramAttachmentMessage.From!.Id,
            telegramAttachmentMessage.Chat.Id,
            new MessageDetails(
                telegramAttachmentMessage.Date,
                BotType.Submissions,
                !string.IsNullOrWhiteSpace(telegramAttachmentMessage.Caption)
                    ? telegramAttachmentMessage.Caption
                    : Option<string>.None(),
                expectedAttachmentExternalUrl,
                attachmentType,
                Option<int>.None()));
        
        // Act
        var actualInputMessage = await converter.ConvertMessageOrThrowAsync(
            telegramAttachmentMessage, BotType.Submissions);
        
        // Assert
        actualInputMessage.Should().BeEquivalentTo(expectedInputMessage);
    }
    
    [Fact]
    public async Task ConvertMessage_ThrowsException_WhenUserIsNull_ForAnyBotType()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var telegramMessage = new Message { From = null, Text = "not empty" };
        
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(
            new TelegramFilePathResolver(mockBotClient.Object),
            _services.GetRequiredService<IUiTranslator>());
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageOrThrowAsync(telegramMessage, BotType.Submissions);

        // Assert
        await convertMessage.Should().ThrowAsync<ToModelConversionException>();
    }
    
    [Fact]
    public async Task ConvertMessage_ThrowsException_WhenTextAndAttachmentFileIdBothEmpty_ForAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var telegramMessage = new Message { From = new User { Id = 123L } };
        
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(
            new TelegramFilePathResolver(mockBotClient.Object),
            _services.GetRequiredService<IUiTranslator>());
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageOrThrowAsync(telegramMessage, BotType.Submissions);

        // Assert
        await convertMessage.Should().ThrowAsync<ToModelConversionException>();
    }

    [Fact]
    public async Task ConvertMessage_ThrowsException_WhenUnsupportedAttachmentTypeLikeVoiceIsSent_ToAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var voiceMessage = utils.GetValidTelegramVoiceMessage();

        var mockBotClient = new Mock<IBotClientWrapper>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(
            new TelegramFilePathResolver(mockBotClient.Object),
            _services.GetRequiredService<IUiTranslator>());

        // Act
        Func<Task<InputMessage>> convertMessage = async () =>
            await converter.ConvertMessageOrThrowAsync(voiceMessage, BotType.Submissions);
        
        // Assert
        await convertMessage.Should().ThrowAsync<ToModelConversionException>();
    }
}
