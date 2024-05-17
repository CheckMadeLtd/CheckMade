using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor : ICommunicationsRequestProcessor
{
    public Task<Attempt<string>> SafelyEchoAsync(InputMessage message)
    {
        return Task.FromResult(Attempt<string>.Run(() => 
            $"Echo from bot Communications: {message.Details.Text.GetValueOrDefault()}"));
    }
}