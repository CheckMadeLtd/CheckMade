using CheckMade.Interfaces;

namespace CheckMade.Messaging.Logic;

public class ResponseGenerator : IResponseGenerator
{
    public string Echo(string input)
    {
        return $"Echo: {input}";
    }
}