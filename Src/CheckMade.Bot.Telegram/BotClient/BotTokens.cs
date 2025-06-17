namespace CheckMade.Bot.Telegram.BotClient;

public sealed record BotTokens(
    string OperationsBotToken,
    string CommunicationsBotToken,
    string NotificationsBotToken);