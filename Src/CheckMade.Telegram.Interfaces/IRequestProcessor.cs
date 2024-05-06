using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IRequestProcessor
{
    public string Echo(InputTextMessage message);
}