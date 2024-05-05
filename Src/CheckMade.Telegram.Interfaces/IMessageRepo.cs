namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepo
{
    void Add(long telegramUserId, string messageText);
}