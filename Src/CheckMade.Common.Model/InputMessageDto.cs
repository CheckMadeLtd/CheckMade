namespace CheckMade.Common.Model;

public record InputMessageDto(
     TelegramUserId UserId,
     TelegramChatId TelegramChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     InputMessageDetails Details);
     