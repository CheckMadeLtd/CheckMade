using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IBotUpdateHandler
{
    Task HandleUpdateAsync(Update update, BotType botType);
}

public class BotUpdateHandler(
        IBotClientFactory botClientFactory,
        IRequestProcessor requestProcessor,
        IToModelConverter converter,
        INetworkRetryPolicy retryPolicy,
        ILogger<BotUpdateHandler> logger) 
    : IBotUpdateHandler
{
    internal const string CallToActionMessageAfterErrorReport = "Please report to your supervisor or contact support.";
    internal const string DataAccessExceptionErrorMessageStub = "An error has occured during a data access operation.";
    
    public async Task HandleUpdateAsync(Update update, BotType botType)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
            case UpdateType.EditedMessage:
                await HandleMessageAsync(update.Message!, botType);
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

    private async Task HandleMessageAsync(Message telegramInputMessage, BotType botType)
    {
        logger.LogInformation("Invoked telegram update function for BotType: {botType} " +
                              "with Message from UserId/ChatId: {userId}/{chatId}", 
            botType, telegramInputMessage.From?.Id ?? 0 ,telegramInputMessage.Chat.Id);

        InputMessage? inputMessage = null;
        
        try
        {
            inputMessage = converter.ConvertMessage(telegramInputMessage);
        }
        catch (Exception ex)
        {
            throw new ToModelConversionException("Failed to convert Telegram Message to Model", ex);
        }
        
        string outputMessage;

        try
        {
            outputMessage = await requestProcessor.EchoAsync(inputMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = ex switch
            { 
                DataAccessException => DataAccessExceptionErrorMessageStub,
                _ => $"A general error (type: {ex.GetType()}) has occured."
            };
            
            logger.LogError(ex, "{errMsg} Next, some details for debugging. " +
                                "BotType: {botType}; UserId: {userId}; " +
                                "DateTime of Input Message: {telegramDate}; " +
                                "Text of InputMessage: {text}", 
                errorMessage, botType, inputMessage.UserId, 
                inputMessage.Details.TelegramDate, inputMessage.Details.Text);
            
            outputMessage = $"{errorMessage} {CallToActionMessageAfterErrorReport}";
        }

        var botClient = botClientFactory.CreateBotClient(botType);

        /* Telegram Servers have queues and handle retrying for sending from itself to end user, but this doesn't
         catch earlier network issues like from our Azure Function to the Telegram Servers! */
        try
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                await botClient.SendTextMessageAsync(
                    chatId: telegramInputMessage.Chat.Id,
                    text: outputMessage);
            });
        }
        catch (Exception ex)
        {
            throw new NetworkAccessException("Failed to reach Telegram servers.", ex);
        }
    }
}
