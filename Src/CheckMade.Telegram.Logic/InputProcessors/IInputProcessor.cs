using CheckMade.Common.Interfaces.Persistence.Tlg;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.Output;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Telegram.Logic.InputProcessors;

public interface IInputProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public static readonly UiString AuthenticateWithToken = Ui("🌀 Please enter your 'role token' to authenticate: ");

    public Task<IReadOnlyList<OutputDto>> ProcessInputAsync(Result<TlgInput> tlgInput);

    protected static async Task<bool> IsUserAuthenticated(
        TlgClientPort inputPort, ITlgClientPortToRoleMapRepository mapRepo)
    {
        IReadOnlyList<TlgClientPortToRoleMap> tlgClientPortToRoleMap =
            (await mapRepo.GetAllAsync()).ToList().AsReadOnly();

        return tlgClientPortToRoleMap
                   .FirstOrDefault(map => map.ClientPort.ChatId == inputPort.ChatId &&
                                          map.Status == DbRecordStatus.Active) 
               != null;
    }
}