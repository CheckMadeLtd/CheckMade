using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.UpdateProcessors;

public interface IUpdateProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public static readonly UiString AuthenticateWithToken = Ui("🌀 Please enter your 'role token' to authenticate: ");

    public Task<IReadOnlyList<OutputDto>> ProcessUpdateAsync(Result<TelegramUpdate> telegramUpdate);
}