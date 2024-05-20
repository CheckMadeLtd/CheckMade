namespace CheckMade.Common.Persistence;

public record UpdateDetails(
    long UserId, 
    string TelegramDateString, 
    IDictionary<string, string> NewValueByColumn);