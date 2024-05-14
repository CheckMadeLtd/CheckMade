using CheckMade.Common.LanguageExtensions.MonadicWrappers;

namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    Option<string> Text,
    Option<string> AttachmentExternalUrl,
    Option<AttachmentType> AttachmentType);