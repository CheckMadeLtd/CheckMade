namespace CheckMade.Telegram.Interfaces;

public interface IRequestProcessor
{
    public string Echo(long telegramUserId, string input);
}