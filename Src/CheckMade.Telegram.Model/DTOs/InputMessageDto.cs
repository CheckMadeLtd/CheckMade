namespace CheckMade.Telegram.Model.DTOs;

public record InputMessageDto(
     TelegramUserId UserId,
     TelegramChatId TelegramChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     InputMessageDetails Details);
     