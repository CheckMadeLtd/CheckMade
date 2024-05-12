namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    string? Text,
    string? AttachmentSourceUrl = null,
    AttachmentType AttachmentType = AttachmentType.NotApplicable);