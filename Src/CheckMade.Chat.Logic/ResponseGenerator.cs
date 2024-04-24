using CheckMade.Common.Interfaces;

namespace CheckMade.Chat.Logic;

public record ResponseGenerator(ITelegramMessageRepo Repo) : IResponseGenerator
{
    public string Echo(long telegramUserId, string input)
    {
        Repo.Add(telegramUserId, input);
        return $"Echo: {input}";
    }
}