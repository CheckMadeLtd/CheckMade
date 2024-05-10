using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

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
        if (update.Message is not { } telegramInputMessage) 
            throw new InvalidOperationException("Right now, only updates with a 'Message' can be handled.");

        logger.LogInformation("Invoked telegram update function for: {botType} " +
                              "with Message from ChatId: {ChatId}", 
            botType, telegramInputMessage.Chat.Id);

        var inputMessage = converter.ConvertMessage(telegramInputMessage);
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
        await retryPolicy.ExecuteAsync(async () =>
        {
            await botClient.SendTextMessageAsync(
                chatId: telegramInputMessage.Chat.Id,
                text: outputMessage);
        });
    }
}
