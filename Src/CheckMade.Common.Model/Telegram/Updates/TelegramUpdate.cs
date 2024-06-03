namespace CheckMade.Common.Model.Telegram.Updates;

public record TelegramUpdate(
     TelegramUserId UserId,
     TelegramChatId TelegramChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     TelegramUpdateDetails Details);
     