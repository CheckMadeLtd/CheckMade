using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor : ICommunicationsRequestProcessor
{
    public Task<string> EchoAsync(InputMessage message)
    {
        return Task.FromResult($"Echo from bot Communications: {message.Details.Text.GetValueOrDefault()}");
    }
}