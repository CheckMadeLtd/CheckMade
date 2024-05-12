using CheckMade.Telegram.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IToModelConverter
{
    Task<InputMessage> ConvertMessageAsync(Message telegramInputMessage);
}

internal class ToModelConverter(ITelegramFilePathResolver filePathResolver) : IToModelConverter
{
    public async Task<InputMessage> ConvertMessageAsync(Message telegramInputMessage)
    {
        var userId = telegramInputMessage.From?.Id 
                     ?? throw new ArgumentNullException(nameof(telegramInputMessage),
                         "From.Id in the input message must not be null");

        var rawAttachmentDetails = ConvertRawAttachmentDetails(telegramInputMessage);
        
        if (string.IsNullOrWhiteSpace(telegramInputMessage.Text) &&
            string.IsNullOrWhiteSpace(rawAttachmentDetails.fileId))
        {
            throw new ArgumentNullException(nameof(telegramInputMessage), "The message must either have " +
                                                                          "a text or an attachment");
        }

        var telegramAttachmentUrl = rawAttachmentDetails.fileId != null 
            ? await filePathResolver.GetTelegramFilePathAsync(rawAttachmentDetails.fileId)
            : null;

        var messageText = !string.IsNullOrWhiteSpace(telegramInputMessage.Text)
            ? telegramInputMessage.Text
            : telegramInputMessage.Caption;
        
        return new InputMessage(
            userId,
            new MessageDetails(
                TelegramDate: telegramInputMessage.Date,
                Text: messageText,
                AttachmentExternalUrl: telegramAttachmentUrl,
                AttachmentType: rawAttachmentDetails.type ));
    }

    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
    private (string? fileId, AttachmentType type) ConvertRawAttachmentDetails(Message telegramInputMessage) => 
        telegramInputMessage.Type switch
    {
        MessageType.Text => (null, AttachmentType.NotApplicable),
        MessageType.Audio => (telegramInputMessage.Audio?.FileId, AttachmentType.Audio),
        MessageType.Photo => (telegramInputMessage.Photo?.OrderBy(p => p.FileSize).Last().FileId, 
            AttachmentType.Photo),
        MessageType.Document => (telegramInputMessage.Document?.FileId, AttachmentType.Document),
        MessageType.Video => (telegramInputMessage.Video?.FileId, AttachmentType.Video),
        _ => throw new ArgumentOutOfRangeException()
    };
}
