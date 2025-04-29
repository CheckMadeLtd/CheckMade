using CheckMade.Common.LangExt.FpExtensions.Monads;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

public sealed record AttachmentSendOutParameters(
    ChatId ChatId,
    InputFileStream FileStream,
    Option<string> Caption, 
    Option<ReplyMarkup> ReplyMarkup);