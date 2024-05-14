using CheckMade.Common.LanguageExtensions.MonadicWrappers;
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
    public async Task ConvertMessage_ConvertsWithCorrectDetails_ForValidTextMessage(string textInput)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        var telegramInputMessage = utils.GetValidTelegramTextMessage(textInput);
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));

        var expectedInputMessage = new InputMessage(
            telegramInputMessage.From!.Id,
            telegramInputMessage.Chat.Id,
            new MessageDetails(
                telegramInputMessage.Date,
                !string.IsNullOrWhiteSpace(telegramInputMessage.Text) 
                    ? telegramInputMessage.Text 
                    : Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None()));

        // Act
        var actualInputMessage = await converter.ConvertMessageAsync(telegramInputMessage);

        // Assert
        actualInputMessage.Should().BeEquivalentTo(expectedInputMessage);
    }
    
    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    [InlineData(AttachmentType.Video)]
    public async Task ConvertMessage_ConvertsWithCorrectDetails_ForValidAttachmentMessage(AttachmentType attachmentType)
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
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));

        var expectedAttachmentExternalUrl =
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{mockBotClient.Object.BotToken}/" +
            $"{(await mockBotClient.Object.GetFileAsync("any")).FilePath}";

        var expectedInputMessage = new InputMessage(
            telegramAttachmentMessage.From!.Id,
            telegramAttachmentMessage.Chat.Id,
            new MessageDetails(
                telegramAttachmentMessage.Date,
                !string.IsNullOrWhiteSpace(telegramAttachmentMessage.Caption)
                    ? telegramAttachmentMessage.Caption
                    : Option<string>.None(),
                expectedAttachmentExternalUrl,
                attachmentType));
        
        // Act
        var actualInputMessage = await converter.ConvertMessageAsync(telegramAttachmentMessage);
        
        // Assert
        actualInputMessage.Should().BeEquivalentTo(expectedInputMessage);
    }
    
    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenUserIsNull()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var telegramMessage = new Message { From = null, Text = "not empty" };
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageAsync(telegramMessage);

        // Assert
        convertMessage.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenTextAndAttachmentFileIdBothEmpty()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var telegramMessage = new Message { From = new User { Id = 123L } };
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageAsync(telegramMessage);

        // Assert
        convertMessage.Should().ThrowAsync<ArgumentNullException>();
    }
}