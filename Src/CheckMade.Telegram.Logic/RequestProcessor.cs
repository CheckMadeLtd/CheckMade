using CheckMade.Telegram.Interfaces;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Logic;

public record RequestProcessor : IRequestProcessor
{
    private readonly IMessageRepo _repo;
    
    public RequestProcessor(IMessageRepo repo)
    {
        _repo = repo;
    }
    
    public string Echo(Message message)
    {
        _repo.Add(message);
        return $"Echo v0.6.1: {message.Text}";
    }
}