using CheckMade.Common.LanguageExtensions.MonadicWrappers;

namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    Option<string> Text,
    AttachmentType AttachmentType = AttachmentType.NotApplicable,
    string? AttachmentExternalUrl = null);