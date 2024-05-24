using CheckMade.Common.Interfaces;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface ICommunicationsRequestProcessor : IRequestProcessor; 

public class CommunicationsRequestProcessor(IUiTranslator translator) : ICommunicationsRequestProcessor
{
    public Task<Attempt<string>> SafelyEchoAsync(InputMessage inputMessage)
    {
        return Attempt<string>.RunAsync(() => 
            Task.FromResult(inputMessage.Details.BotCommandEnumCode.GetValueOrDefault() == Start.CommandCode 
                ? translator.Translate(UiConcatenate(
                    Ui("Willkommen zum {0} Bot! ", BotType.Communications), 
                    IRequestProcessor.WelcomeToBotMenuInstruction)) 
                : translator.Translate(
                    Ui($"Echo from bot Communications: {inputMessage.Details.Text.GetValueOrDefault()}"))));
    }
}