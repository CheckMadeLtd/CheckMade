namespace CheckMade.Common.Persistence;

public record UpdateDetails(int Id, IDictionary<string, string> NewValueByColumn);