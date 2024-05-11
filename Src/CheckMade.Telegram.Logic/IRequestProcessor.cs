using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic;

public interface IRequestProcessor
{
    public Task<string> EchoAsync(InputMessage message);
}