
namespace CheckMade.Telegram.Model.DTOs;

public record InputMessageDetails(
    DateTime TelegramDate,
    Option<string> Text,
    Option<string> AttachmentExternalUrl,
    Option<AttachmentType> AttachmentType,
    Option<int> BotCommandEnumCode);