
using CheckMade.Common.LangExt.MonadicWrappers;

namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    BotType RecipientBotType,
    Option<string> Text,
    Option<string> AttachmentExternalUrl,
    Option<AttachmentType> AttachmentType,
    Option<int> BotCommandEnumCode);