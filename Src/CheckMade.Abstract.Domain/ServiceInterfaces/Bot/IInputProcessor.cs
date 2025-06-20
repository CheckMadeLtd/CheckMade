using CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;
using CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.ServiceInterfaces.Bot;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<(Option<Input> EnrichedOriginalInput, IReadOnlyCollection<Output> ResultingOutputs)> 
        ProcessInputAsync(Result<Input> input);
}
