using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic;

public class RequestProcessor(IMessageRepo repo) : IRequestProcessor
{
    public async Task<string> EchoAsync(InputTextMessage message)
    {
        await repo.AddAsync(message);
        return $"Echo v0.6.1: {message.Details.Text}";
    }
}