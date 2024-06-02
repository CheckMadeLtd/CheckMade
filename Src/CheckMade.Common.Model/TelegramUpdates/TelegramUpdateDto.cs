namespace CheckMade.Common.Model.TelegramUpdates;

public record TelegramUpdateDto(
     TelegramUserId UserId,
     TelegramChatId TelegramChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     InputMessageDetails Details);
     