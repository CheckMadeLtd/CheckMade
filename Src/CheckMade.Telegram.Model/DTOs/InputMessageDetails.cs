
namespace CheckMade.Telegram.Model.DTOs;

public record InputMessageDetails(
    DateTime TelegramDate,
    int TelegramMessageId,
    Option<string> Text,
    Option<string> AttachmentExternalUrl,
    Option<AttachmentType> AttachmentType,
    Option<int> BotCommandEnumCode,
    Option<int> DomainCategoryEnumCode,
    Option<long> ControlPromptEnumCode);