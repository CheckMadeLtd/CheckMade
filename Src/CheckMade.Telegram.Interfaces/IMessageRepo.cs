using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IMessageRepo
{
    void Add(InputTextMessage inputMessage);
    IEnumerable<InputTextMessage> GetAll(long userId);
}