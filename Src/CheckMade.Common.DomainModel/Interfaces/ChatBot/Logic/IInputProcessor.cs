using CheckMade.Common.DomainModel.Data.ChatBot.Input;
using CheckMade.Common.DomainModel.Data.ChatBot.Output;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Interfaces.ChatBot.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<(Option<TlgInput> EnrichedOriginalInput, IReadOnlyCollection<OutputDto> ResultingOutputs)> 
        ProcessInputAsync(Result<TlgInput> input);
}
