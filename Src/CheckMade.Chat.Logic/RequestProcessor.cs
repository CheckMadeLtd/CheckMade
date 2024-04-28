using CheckMade.Common.Interfaces;

namespace CheckMade.Chat.Logic;

public record RequestProcessor : IRequestProcessor
{
    private readonly ITelegramMessageRepo _repo;
    
    public RequestProcessor(ITelegramMessageRepo repo)
    {
        _repo = repo;
    }
    
    public string Echo(long telegramUserId, string input)
    {
        _repo.Add(telegramUserId, input);
        return $"Echo: {input}";
    }
}