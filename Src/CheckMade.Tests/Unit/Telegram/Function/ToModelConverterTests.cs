using CheckMade.Telegram.Function.Services.BotClient;
using CheckMade.Telegram.Function.Services.Conversions;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.DTOs;
using CheckMade.Tests.Startup;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Telegram.Bot.Types;

namespace CheckMade.Tests.Unit.Telegram.Function;

public class ToModelConverterTests
{
    private ServiceProvider? _services;

    [Theory]
    [InlineData("Normal valid text message")]
    [InlineData("_")]
    [InlineData(" valid text message \n with line break and trailing spaces ")]
    public async Task ConvertMessageAsync_ConvertsWithCorrectDetails_ForValidTextMessage_ToAnyBotType(
        string textInput)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var telegramInputMessage = basics.utils.GetValidTelegramTextMessage(textInput);

        var expectedInputMessage = new InputMessageDto(
            telegramInputMessage.From!.Id,
            telegramInputMessage.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                telegramInputMessage.Date,
                telegramInputMessage.MessageId,
                !string.IsNullOrWhiteSpace(telegramInputMessage.Text) 
                    ? telegramInputMessage.Text 
                    : Option<string>.None(),
                Option<string>.None(),
                Option<AttachmentType>.None(),
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));

        var actualInputMessage = 
            await basics.converter.ConvertMessageAsync(telegramInputMessage, BotType.Submissions);

        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }
    
    [Theory]
    [InlineData(AttachmentType.Photo)]
    [InlineData(AttachmentType.Audio)]
    [InlineData(AttachmentType.Document)]
    [InlineData(AttachmentType.Video)]
    public async Task ConvertMessageAsync_ConvertsWithCorrectDetails_ForValidAttachmentMessage_ToAnyBotType(
        AttachmentType attachmentType)
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var telegramAttachmentMessage = attachmentType switch
        {
            AttachmentType.Audio => basics.utils.GetValidTelegramAudioMessage(),
            AttachmentType.Document => basics.utils.GetValidTelegramDocumentMessage(),
            AttachmentType.Photo => basics.utils.GetValidTelegramPhotoMessage(),
            AttachmentType.Video => basics.utils.GetValidTelegramVideoMessage(),
            _ => throw new ArgumentOutOfRangeException(nameof(attachmentType))
        };
        
        var expectedAttachmentExternalUrl =
            TelegramFilePathResolver.TelegramBotDownloadFileApiUrlStub + $"bot{basics.mockBotClient.Object.BotToken}/" +
            $"{(await basics.mockBotClient.Object.GetFileOrThrowAsync("any")).FilePath}";

        var expectedInputMessage = new InputMessageDto(
            telegramAttachmentMessage.From!.Id,
            telegramAttachmentMessage.Chat.Id,
            BotType.Submissions,
            new InputMessageDetails(
                telegramAttachmentMessage.Date,
                telegramAttachmentMessage.MessageId,
                !string.IsNullOrWhiteSpace(telegramAttachmentMessage.Caption)
                    ? telegramAttachmentMessage.Caption
                    : Option<string>.None(),
                expectedAttachmentExternalUrl,
                attachmentType,
                Option<int>.None(),
                Option<int>.None(), 
                Option<long>.None()));
        
        var actualInputMessage = await basics.converter.ConvertMessageAsync(
            telegramAttachmentMessage, BotType.Submissions);
        
        Assert.Equivalent(expectedInputMessage, actualInputMessage.GetValueOrDefault());
    }
    
    [Fact]
    public async Task ConvertMessageAsync_ReturnsFailure_WhenUserIsNull_ForAnyBotType()
    {
         _services = new UnitTestStartup().Services.BuildServiceProvider();
         var basics = GetBasicTestingServices(_services);
         
        var telegramMessage = new Message { From = null, Text = "not empty" };
        var conversionAttempt = await basics.converter.ConvertMessageAsync(telegramMessage, BotType.Submissions);
        Assert.True(conversionAttempt.IsFailure);
    }
    
    [Fact]
    public async Task ConvertMessageAsync_ReturnsFailure_WhenTextAndAttachmentFileIdBothEmpty_ForAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var telegramMessage = new Message { From = new User { Id = 123L } };
        var conversionAttempt = await basics.converter.ConvertMessageAsync(telegramMessage, BotType.Submissions);
        Assert.True(conversionAttempt.IsFailure);
    }

    [Fact]
    public async Task ConvertMessageAsync_ReturnsFailure_WhenUnsupportedAttachmentTypeLikeVoiceSent_ToAnyBotType()
    {
        _services = new UnitTestStartup().Services.BuildServiceProvider();
        var basics = GetBasicTestingServices(_services);
        
        var voiceMessage = basics.utils.GetValidTelegramVoiceMessage();
        var conversionAttempt = await basics.converter.ConvertMessageAsync(voiceMessage, BotType.Submissions);

        Assert.True(conversionAttempt.IsFailure);
        Assert.Equal("Failed to convert Telegram Message to Model. Attachment type Voice is not yet supported!",
            conversionAttempt.Failure!.Error!.GetFormattedEnglish());
    }

    private static (ITestUtils utils, Mock<IBotClientWrapper> mockBotClient, IToModelConverter converter)
        GetBasicTestingServices(IServiceProvider sp)
    {
        var utils = sp.GetRequiredService<ITestUtils>();
        var mockBotClient = sp.GetRequiredService<Mock<IBotClientWrapper>>();
        var converterFactory = sp.GetRequiredService<IToModelConverterFactory>();
        var converter = converterFactory.Create(new TelegramFilePathResolver(mockBotClient.Object));

        return (utils, mockBotClient, converter);
    }
}
