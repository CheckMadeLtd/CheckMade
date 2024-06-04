using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace CheckMade.Telegram.Function.Services.BotClient;

public record AttachmentSendOutParameters(
    ChatId DestinationChatId,
    InputFileStream FileStream,
    Option<string> Caption, 
    Option<IReplyMarkup> ReplyMarkup);