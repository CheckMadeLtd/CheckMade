using CheckMade.Common.LangExt.MonadicWrappers;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor : ICommunicationsRequestProcessor
{
    public Task<Attempt<string>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return Attempt<string>.RunAsync(() => 
            Task.FromResult(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode 
                ? string.Format(IRequestProcessor.WelcomeToBot, BotType.Communications) 
                : Ui($"Echo from bot Communications: {inputMessage.Details.Text.GetValueOrDefault()}")));
    }
}