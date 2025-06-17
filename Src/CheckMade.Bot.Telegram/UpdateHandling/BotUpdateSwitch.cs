using CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Bot.Telegram.UpdateHandling;

public interface IBotUpdateSwitch
{
    Task<Result<Unit>> SwitchUpdateAsync(Update update, InteractionMode interactionMode);
}

public sealed class BotUpdateSwitch(IUpdateHandler updateHandler, ILogger<BotUpdateSwitch> logger) : IBotUpdateSwitch
{
    internal static readonly UiString
        NoSpecialHandlingWarning = Ui("""
                                      Telegram Message/Update of this type not yet supported.
                                      No special handling is taking place for it, but that doesn't mean that 
                                      a Telegram-System-related update didn't work. You may assume it did.
                                      """);
    
    public async Task<Result<Unit>> SwitchUpdateAsync(Update update, InteractionMode interactionMode)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (update.Type)
        {
            case UpdateType.Message:
            case UpdateType.EditedMessage:
            case UpdateType.CallbackQuery:
                return await updateHandler.HandleUpdateAsync(new UpdateWrapper(update), interactionMode);

            case UpdateType.MyChatMember:
                logger.LogInformation("MyChatMember Update from '{From}', with previous status '{OldStatus}' " +
                                      "and new status '{NewStatus}'",
                    update.MyChatMember!.From.Username, 
                    update.MyChatMember.OldChatMember.Status, 
                    update.MyChatMember.NewChatMember.Status);
                return Unit.Value;
            
            default:
                logger.LogWarning("Received update of type '{updateType}': {warningMessage}", 
                    update.Type, NoSpecialHandlingWarning.GetFormattedEnglish());
                return Unit.Value;
        }
    }
}
