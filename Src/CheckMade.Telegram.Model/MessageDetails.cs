namespace CheckMade.Telegram.Model;

public record MessageDetails(
    DateTime TelegramDate,
    string? Text,
    string? AttachmentUrl = null,
    AttachmentType AttachmentType = AttachmentType.NotApplicable);