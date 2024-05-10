using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic;

public class RequestProcessor(IMessageRepository repo) : IRequestProcessor
{
    public async Task<string> EchoAsync(InputMessage message)
    {
        await repo.AddAsync(message);
        return $"Echo: {message.Details.Text}";
    }
}