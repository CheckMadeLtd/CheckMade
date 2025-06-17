using General.Utils.FpExtensions.Monads;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Bot.Telegram.UpdateHandling;

public sealed record AttachmentSendOutParameters(
    ChatId ChatId,
    InputFileStream FileStream,
    Option<string> Caption, 
    Option<ReplyMarkup> ReplyMarkup);