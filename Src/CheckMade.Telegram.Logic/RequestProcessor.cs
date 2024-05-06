using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic;

public record RequestProcessor : IRequestProcessor
{
    private readonly IMessageRepo _repo;
    
    public RequestProcessor(IMessageRepo repo)
    {
        _repo = repo;
    }
    
    public string Echo(InputTextMessage message)
    {
        _repo.Add(message);
        return $"Echo v0.6.1: {message.Details.Text}";
    }
}