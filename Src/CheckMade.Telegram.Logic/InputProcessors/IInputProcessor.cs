using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Common.Model.Tlg.Output;

namespace CheckMade.Telegram.Logic.InputProcessors;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public static readonly UiString AuthenticateWithToken = Ui("🌀 Please enter your 'role token' to authenticate: ");

    public Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput);
}