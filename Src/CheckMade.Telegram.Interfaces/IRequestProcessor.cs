using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Interfaces;

public interface IRequestProcessor
{
    public Task<string> EchoAsync(InputMessage message);
}