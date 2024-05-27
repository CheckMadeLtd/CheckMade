namespace CheckMade.Telegram.Model;

public record InputMessage(
     long UserId,
     long ChatId,
     BotType BotType,
     MessageDetails Details);
     