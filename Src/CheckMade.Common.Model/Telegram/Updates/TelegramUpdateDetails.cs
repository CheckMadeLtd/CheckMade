
namespace CheckMade.Common.Model.Telegram.Updates;

public record TelegramUpdateDetails(
    DateTime TelegramDate,
    int TelegramMessageId,
    Option<string> Text,
    Option<Uri> AttachmentTelegramUri,
    Option<Uri> AttachmentInternalUri,
    Option<AttachmentType> AttachmentType,
    Option<Geo> GeoCoordinates,
    Option<int> BotCommandEnumCode,
    Option<int> DomainCategoryEnumCode,
    Option<long> ControlPromptEnumCode);