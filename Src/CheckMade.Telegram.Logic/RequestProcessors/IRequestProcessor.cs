using CheckMade.Common.FpExt.MonadicWrappers;
using CheckMade.Telegram.Model;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface IRequestProcessor
{
    public Task<Attempt<string>> SafelyEchoAsync(InputMessage message);
}