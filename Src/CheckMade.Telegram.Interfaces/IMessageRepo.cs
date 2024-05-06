using Telegram.Bot.Types;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepo
{
    void Add(Message message);
    IEnumerable<Message> GetAll(long userId);
}