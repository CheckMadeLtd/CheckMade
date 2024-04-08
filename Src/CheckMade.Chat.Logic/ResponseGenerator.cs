using CheckMade.Common.Interfaces;

namespace CheckMade.Chat.Logic;

public record ResponseGenerator : IResponseGenerator
{
    public string Echo(string input)
    {
        return $"Echo: {input}";
    }
}