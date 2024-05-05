namespace CheckMade.Telegram.Interfaces;

public interface ITelegramMessageRepo
{
    void Add(long telegramUserId, string messageText);
}