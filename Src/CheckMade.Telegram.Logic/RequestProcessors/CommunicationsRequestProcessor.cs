using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor : ICommunicationsRequestProcessor
{
    public Task<Attempt<UiString>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return Attempt<UiString>.RunAsync(() => 
            Task.FromResult(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode 
                ? UiConcatenate(
                    Ui("Willkommen zum {0} Bot! ", BotType.Communications), 
                    IRequestProcessor.WelcomeToBotMenuInstruction) 
                : Ui($"Echo from bot Communications: {inputMessage.Details.Text.GetValueOrDefault()}")));
    }
}