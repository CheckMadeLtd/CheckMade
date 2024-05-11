using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services;

public interface IMessageHandler
{
    Task HandleMessageAsync(Message telegramInputMessage, BotType botType);
}

public class MessageHandler(IBotClientFactory botClientFactory,
        IRequestProcessor requestProcessor,
        IToModelConverter converter,
        INetworkRetryPolicy retryPolicy,
        ILogger<MessageHandler> logger)
    : IMessageHandler
{
    internal const string CallToActionMessageAfterErrorReport = "Please report to your supervisor or contact support.";
    internal const string DataAccessExceptionErrorMessageStub = "An error has occured during a data access operation.";

    public async Task HandleMessageAsync(Message telegramInputMessage, BotType botType)
    {
        logger.LogInformation("Invoked telegram update function for BotType: {botType} " +
                              "with Message from UserId/ChatId: {userId}/{chatId}", 
            botType, telegramInputMessage.From?.Id ?? 0 ,telegramInputMessage.Chat.Id);

        // switch (telegramInputMessage.Type)
        // {
        //     case MessageType.Text:
        //         return;
        // }
        
        InputMessage? inputMessage;
        
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