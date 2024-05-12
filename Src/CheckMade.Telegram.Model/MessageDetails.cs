namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    string? Text,
    AttachmentType AttachmentType = AttachmentType.NotApplicable,
    string? AttachmentExternalUrl = null);