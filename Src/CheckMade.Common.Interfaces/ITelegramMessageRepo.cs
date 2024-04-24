namespace CheckMade.Common.Interfaces;

public interface ITelegramMessageRepo
{
    void Add(long telegramUserId, string messageText);
}