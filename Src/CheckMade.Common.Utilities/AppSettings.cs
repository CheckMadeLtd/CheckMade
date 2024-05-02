namespace CheckMade.Common.Utilities;

public record AppSettings(
    string TelegramSubmissionsBotToken,
    string TelegramCommunicationsBotToken,
    string TelegramNotificationsBotToken);