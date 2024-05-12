using System.ComponentModel;
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

        var attachmentFileId = GetAttachmentFileId(telegramInputMessage);
        
        if (string.IsNullOrWhiteSpace(telegramInputMessage.Text) &&
            string.IsNullOrWhiteSpace(attachmentFileId))
        {
            throw new ArgumentNullException(nameof(telegramInputMessage), "The message must either have " +
                                                                          "a text or an attachment");
        }

        // ToDo: Fix, this should become the azure storage URL
        var attachmentUrl = !string.IsNullOrWhiteSpace(attachmentFileId)
            ? await GetTelegramFilePathAsync(attachmentFileId, botClient)
            : null;
        
        return new InputMessage(
            userId,
            new MessageDetails(
                TelegramDate: telegramInputMessage.Date,
                Text: telegramInputMessage.Text,
                AttachmentUrl: attachmentUrl,
                AttachmentType: attachmentUrl == null ? AttachmentType.NotApplicable : AttachmentType.Photo ));
    }

    private string? GetAttachmentFileId(Message telegramInputMessage) => telegramInputMessage.Type switch
    {
        MessageType.Photo => telegramInputMessage.Photo?.OrderBy(p => p.FileSize).Last().FileId,

        _ => throw new ArgumentOutOfRangeException()
    };
    
    private async Task<string> GetTelegramFilePathAsync(string fileId, IBotClientWrapper botClient)
    {
        var file = await botClient.GetFileAsync(fileId);
        return TelegramBotDownloadFileApiUrlStub + $"bot{botClient.BotToken}/{file.FilePath}";  
    }
}
