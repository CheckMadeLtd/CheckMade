namespace CheckMade.Telegram.Model.DTOs;

public record InputMessageDto(
     long UserId,
     long ChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     InputMessageDetails Details);
     