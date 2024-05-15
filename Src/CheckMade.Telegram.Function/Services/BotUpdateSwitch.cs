using CheckMade.Common.LanguageExtensions;
using CheckMade.Common.LanguageExtensions.MonadicWrappers;
using CheckMade.Telegram.Logic;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IBotUpdateSwitch
{
    Task<Attempt<Unit>> HandleUpdateAsync(Update update, BotType botType);
}

public class BotUpdateSwitch(IMessageHandler messageHandler, ILogger<BotUpdateSwitch> logger) : IBotUpdateSwitch
{
    internal const string
        NoSpecialHandlingWarningMessage = "Telegram Message/Update of this type not yet supported. " +
                                          "No special handling is taking place for it, but that doesn't mean that a " +
                                          "Telegram-System-related update didn't work. You may assume it did.";
    
    public async Task<Attempt<Unit>> HandleUpdateAsync(Update update, BotType botType)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (update.Type)
        {
            case UpdateType.Message:
            case UpdateType.EditedMessage:
                return await messageHandler.HandleMessageAsync(update.Message!, botType);

            case UpdateType.CallbackQuery:
                // ToDo: Implement separate handling of InlineKeyboardResponseReceived
                return Attempt<Unit>.Succeed();

            case UpdateType.MyChatMember:
                logger.LogInformation("MyChatMember Update from '{From}', with previous status '{OldStatus}' " +
                                      "and new status '{NewStatus}'",
                    update.MyChatMember!.From.Username, update.MyChatMember.OldChatMember.Status, 
                    update.MyChatMember.NewChatMember.Status);
                return Attempt<Unit>.Succeed();
            
            default:
                logger.LogWarning("Received update of type '{updateType}': {warningMessage}", 
                    update.Type, NoSpecialHandlingWarningMessage);
                return Attempt<Unit>.Succeed();
        }
    }
}
