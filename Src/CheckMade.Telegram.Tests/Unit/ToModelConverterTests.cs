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
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    [InlineData(AttachmentType.Video)]
    public async Task ConvertMessage_ConvertsWithCorrectDetails_ForValidAttachmentMessage(AttachmentType type)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        
        // Arrange
        var utils = _services.GetRequiredService<ITestUtils>();
        
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        var attachmentMessage = type switch
        {
            AttachmentType.Audio => utils.GetValidAudioMessage(),
            AttachmentType.Document => utils.GetValidDocumentMessage(),
            AttachmentType.Photo => utils.GetValidPhotoMessage(),
            AttachmentType.Video => utils.GetValidVideoMessage(),
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var mockBotClient = _services.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));

        var expectedAttachmentExternalUrl =
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{mockBotClient.Object.BotToken}/" +
            $"{(await mockBotClient.Object.GetFileAsync("any")).FilePath}";
        
        // Act
        var inputMessageResult = await converter.ConvertMessageAsync(attachmentMessage);
        
        // Assert
        inputMessageResult.UserId.Should().Be(attachmentMessage.From!.Id);
        inputMessageResult.Details.TelegramDate.Should().Be(attachmentMessage.Date);
        inputMessageResult.Details.Text.Should().Be(attachmentMessage.Caption);
        inputMessageResult.Details.AttachmentType.Should().Be(type);
        inputMessageResult.Details.AttachmentExternalUrl.Should().Be(expectedAttachmentExternalUrl);
    }
    
    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenUserIsNull()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var message = new Message { From = null, Text = "not empty" };
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageAsync(message);

        // Assert
        convertMessage.Should().ThrowAsync<ArgumentNullException>();
    }
    
    [Fact]
    public void ConvertMessage_ThrowsArgumentNullException_WhenTextAndAttachmentFileIdBothEmpty()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
    
        // Arrange
        var message = new Message { From = new User { Id = 123L } };
        var mockBotClient = new Mock<IBotClientWrapper>();
        var converterFactory = _services.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));
        
        // Act
        Func<Task<InputMessage>> convertMessage = async () => 
            await converter.ConvertMessageAsync(message);

        // Assert
        convertMessage.Should().ThrowAsync<ArgumentNullException>();
    }
}