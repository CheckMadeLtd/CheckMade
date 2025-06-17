using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.Domain.Interfaces.ChatBot.Logic;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<(Option<Input> EnrichedOriginalInput, IReadOnlyCollection<Output> ResultingOutputs)> 
        ProcessInputAsync(Result<Input> input);
}
