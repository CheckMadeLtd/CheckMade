namespace CheckMade.Telegram.Model;

public record InputMessage(
     long UserId,
     long ChatId,
     MessageDetails Details);
     