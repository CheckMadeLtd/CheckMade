using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.DTOs;

namespace CheckMade.Telegram.Logic.RequestProcessors;

public interface IRequestProcessor
{
    public static readonly UiString SeeValidBotCommandsInstruction = 
        Ui("Tap on the menu button or type '/' to see available BotCommands.");

    public Task<Attempt<OutputDto>> SafelyProcessRequestAsync(InputMessageDto inputMessage);
}