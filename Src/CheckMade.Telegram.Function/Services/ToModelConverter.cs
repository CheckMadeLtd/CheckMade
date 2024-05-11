using CheckMade.Telegram.Model;
using Telegram.Bot.Types;

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

        var photoFileId = telegramInputMessage.Photo?.OrderBy(p => p.FileSize).Last().FileId;
        
        if (string.IsNullOrWhiteSpace(telegramInputMessage.Text) &&
            string.IsNullOrWhiteSpace(photoFileId))
        {
            throw new ArgumentNullException(nameof(telegramInputMessage), "The message must either have " +
                                                                          "a text or an attachment");
        }

        // ToDo: Fix, this should become the azure storage URL
        var attachmentUrl = !string.IsNullOrWhiteSpace(photoFileId)
            ? await GetTelegramFilePathAsync(photoFileId, botClient)
            : null;
        
        return new InputMessage(
            userId,
            new MessageDetails(
                TelegramDate: telegramInputMessage.Date,
                Text: telegramInputMessage.Text,
                AttachmentUrl: attachmentUrl,
                AttachmentType: attachmentUrl == null ? AttachmentType.NotApplicable : AttachmentType.Photo ));
    }

    private async Task<string> GetTelegramFilePathAsync(string fileId, IBotClientWrapper botClient)
    {
        var file = await botClient.GetFileAsync(fileId);
        return TelegramBotDownloadFileApiUrlStub + $"bot{botClient.BotToken}/{file.FilePath}";  
    }
}
