namespace CheckMade.Common.Model.TelegramUpdates;

public record TelegramUpdate(
     TelegramUserId UserId,
     TelegramChatId TelegramChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     TelegramUpdateDetails Details);
     