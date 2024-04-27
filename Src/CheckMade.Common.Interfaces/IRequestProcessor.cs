namespace CheckMade.Common.Interfaces;

public interface IRequestProcessor
{
    public string Echo(long telegramUserId, string input);
}