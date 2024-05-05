using CheckMade.Telegram.Interfaces;

namespace CheckMade.Telegram.Logic;

public record RequestProcessor : IRequestProcessor
{
    private readonly IMessageRepo _repo;
    
    public RequestProcessor(IMessageRepo repo)
    {
        _repo = repo;
    }
    
    public string Echo(long telegramUserId, string input)
    {
        _repo.Add(telegramUserId, input);
        return $"Echo: {input}";
    }
}