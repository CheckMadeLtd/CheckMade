using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Telegram.Function.Services.UpdateHandling;

public record AttachmentSendOutParameters(
    ChatId ChatId,
    InputFileStream FileStream,
    Option<string> Caption, 
    Option<IReplyMarkup> ReplyMarkup);