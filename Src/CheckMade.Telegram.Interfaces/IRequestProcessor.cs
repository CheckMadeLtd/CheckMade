using Telegram.Bot.Types;

namespace CheckMade.Telegram.Interfaces;

public interface IRequestProcessor
{
    public string Echo(Message message);
}