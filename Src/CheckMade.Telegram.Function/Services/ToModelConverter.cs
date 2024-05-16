using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Model;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IToModelConverter
{
    Task<InputMessage> ConvertMessageOrThrowAsync(Message telegramInputMessage);
}

internal class ToModelConverter(ITelegramFilePathResolver filePathResolver) : IToModelConverter
{
    public async Task<InputMessage> ConvertMessageOrThrowAsync(Message telegramInputMessage)
    {
        try
        {
            return await ConvertMessageAsync(telegramInputMessage);
        }
        catch (Exception ex)
        {
            throw new ToModelConversionException("Failed to convert Telegram Message to Model", ex);
        }
    }

    private async Task<InputMessage> ConvertMessageAsync(Message telegramInputMessage)
    {
        var userId = telegramInputMessage.From?.Id 
                     ?? throw new ArgumentNullException(nameof(telegramInputMessage),
                         "From.Id in the input message must not be null");
        
        var rawAttachmentDetails = ConvertRawAttachmentDetails(telegramInputMessage);
        
        if (string.IsNullOrWhiteSpace(telegramInputMessage.Text) && rawAttachmentDetails.fileId.IsNone)
        {
            throw new ArgumentNullException(nameof(telegramInputMessage), 
                "The message must either have a text or an attachment");
        }

        var telegramAttachmentUrl = await rawAttachmentDetails.fileId.Match<Task<Option<string>>>(
            async value => await filePathResolver.GetTelegramFilePathAsync(value),
            () => Task.FromResult(Option<string>.None()));        
        
        var messageText = !string.IsNullOrWhiteSpace(telegramInputMessage.Text)
            ? telegramInputMessage.Text
            : telegramInputMessage.Caption;
        
        return new InputMessage(
            userId,
            telegramInputMessage.Chat.Id,
            new MessageDetails(
                telegramInputMessage.Date,
                !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(),
                telegramAttachmentUrl,
                rawAttachmentDetails.type ));
    }
    
    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
    private (Option<string> fileId, Option<AttachmentType> type) ConvertRawAttachmentDetails(Message telegramInputMessage)
    {
        const string errorMessage = "For Telegram message of type {0} we expect the {0} property to not be null";

        return telegramInputMessage.Type switch
        {
            MessageType.Text => (Option<string>.None(), Option<AttachmentType>.None()),
            
            MessageType.Audio => (telegramInputMessage.Audio?.FileId
                                  ?? throw new InvalidOperationException(
                                      string.Format(errorMessage, telegramInputMessage.Type)),
                AttachmentType.Audio),
            
            MessageType.Photo => (telegramInputMessage.Photo?.OrderBy(p => p.FileSize).Last().FileId
                                  ?? throw new InvalidOperationException(
                                      string.Format(errorMessage, telegramInputMessage.Type)),
                AttachmentType.Photo),
            
            MessageType.Document => (telegramInputMessage.Document?.FileId
                                     ?? throw new InvalidOperationException(
                                         string.Format(errorMessage, telegramInputMessage.Type)),
                AttachmentType.Document),
            
            MessageType.Video => (telegramInputMessage.Video?.FileId
                                  ?? throw new InvalidOperationException(
                                      string.Format(errorMessage, telegramInputMessage.Type)),
                AttachmentType.Video),
            
            _ => throw new ArgumentOutOfRangeException(nameof(telegramInputMessage))
        };
    } 
}
