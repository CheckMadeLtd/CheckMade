namespace CheckMade.Telegram.Model.DTOs;

public record InputMessageDto(
     UserId UserId,
     TelegramChatId TelegramChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     InputMessageDetails Details);
     