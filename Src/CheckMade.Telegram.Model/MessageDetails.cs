namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    string? Text,
    string? AttachmentExternalUrl = null,
    AttachmentType AttachmentType = AttachmentType.NotApplicable);