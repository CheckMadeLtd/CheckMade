namespace CheckMade.Common.Model.Telegram.Updates;

public record TelegramUpdate(
     TelegramUserId UserId,
     TelegramChatId ChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     TelegramUpdateDetails Details);
     