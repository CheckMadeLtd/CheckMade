namespace CheckMade.Telegram.Function.Startup;

public record BotTokens(
    string TelegramSubmissionsBotToken,
    string TelegramCommunicationsBotToken,
    string TelegramNotificationsBotToken);