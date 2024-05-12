using CheckMade.Telegram.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IToModelConverter
{
    Task<InputMessage> ConvertMessageAsync(Message telegramInputMessage, IBotClientWrapper botClient);
}

internal class ToModelConverter : IToModelConverter
{
    private const string TelegramBotDownloadFileApiUrlStub = "https://api.telegram.org/file/";
    
    public async Task<InputMessage> ConvertMessageAsync(Message telegramInputMessage, IBotClientWrapper botClient)
    {
        var userId = telegramInputMessage.From?.Id 
                     ?? throw new ArgumentNullException(nameof(telegramInputMessage),
                         "From.Id in the input message must not be null");

        var rawAttachmentDetails = GetRawAttachmentDetails(telegramInputMessage);
        
        if (string.IsNullOrWhiteSpace(telegramInputMessage.Text) &&
            string.IsNullOrWhiteSpace(rawAttachmentDetails.fileId))
        {
            throw new ArgumentNullException(nameof(telegramInputMessage), "The message must either have " +
                                                                          "a text or an attachment");
        }

        var attachmentUrl = rawAttachmentDetails.fileId != null 
            ? await GetTelegramFilePathAsync(rawAttachmentDetails.fileId, botClient)
            : null;

        var messageText = !string.IsNullOrWhiteSpace(telegramInputMessage.Text)
            ? telegramInputMessage.Text
            : telegramInputMessage.Caption;
        
        return new InputMessage(
            userId,
            new MessageDetails(
                TelegramDate: telegramInputMessage.Date,
                Text: messageText,
                AttachmentExternalUrl: attachmentUrl,
                AttachmentType: rawAttachmentDetails.type ));
    }

    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
    private (string? fileId, AttachmentType type) GetRawAttachmentDetails(Message telegramInputMessage) => 
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
    
    private async Task<string> GetTelegramFilePathAsync(string fileId, IBotClientWrapper botClient)
    {
        var file = await botClient.GetFileAsync(fileId);
        return TelegramBotDownloadFileApiUrlStub + $"bot{botClient.BotToken}/{file.FilePath}";  
    }
}
