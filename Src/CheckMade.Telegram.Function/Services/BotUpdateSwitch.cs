using CheckMade.Telegram.Logic;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IBotUpdateSwitch
{
    Task HandleUpdateAsync(Update update, BotType botType);
}

public class BotUpdateSwitch(IMessageHandler messageHandler, ILogger<BotUpdateSwitch> logger) : IBotUpdateSwitch
{
    public async Task HandleUpdateAsync(Update update, BotType botType)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
            case UpdateType.EditedMessage:
                await messageHandler.HandleMessageAsync(update.Message!, botType);
                return;

            case UpdateType.CallbackQuery:
                // ToDo: Implement separate handling of InlineKeyboardResponseReceived
                return;
            
            case UpdateType.MyChatMember:
                logger.LogInformation("MyChatMember Update from '{From}', with previous status '{OldStatus}' " +
                                      "and new status '{NewStatus}'",
                    update.MyChatMember!.From.Username, update.MyChatMember.OldChatMember.Status, 
                    update.MyChatMember.NewChatMember.Status);
                return;
            
            case UpdateType.Unknown:
            case UpdateType.InlineQuery:
            case UpdateType.ChosenInlineResult:
            case UpdateType.ChannelPost:
            case UpdateType.EditedChannelPost:
            case UpdateType.ShippingQuery:
            case UpdateType.PreCheckoutQuery:
            case UpdateType.Poll:
            case UpdateType.PollAnswer:
            case UpdateType.ChatMember:
            case UpdateType.ChatJoinRequest:
            default:
                logger.LogWarning("Telegram Update of type {updateType} not yet supported. No special " +
                                  "processing is taking place for it, but that doesn't mean a Telegram-related" +
                                  "system update didn't work. It probably did.", update.Type);
                return;
        }
    }
}
