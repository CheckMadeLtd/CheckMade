namespace CheckMade.Common.Persistence;

public record UpdateDetails(
    long UserId, 
    DateTime TelegramDate, 
    IDictionary<string, object> NewValueByColumn);