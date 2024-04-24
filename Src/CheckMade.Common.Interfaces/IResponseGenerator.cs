namespace CheckMade.Common.Interfaces;

public interface IResponseGenerator
{
    public string Echo(long telegramUserId, string input);
}