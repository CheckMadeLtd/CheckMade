namespace CheckMade.Telegram.Model.DTOs;

public record InputMessageDto(
     UserId UserId,
     long ChatId,
     BotType BotType,
     ModelUpdateType ModelUpdateType,
     InputMessageDetails Details);
     