using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic;

public class RequestProcessor(IMessageRepo repo) : IRequestProcessor
{
    public string Echo(InputTextMessage message)
    {
        repo.Add(message);
        return $"Echo v0.6.1: {message.Details.Text}";
    }
}