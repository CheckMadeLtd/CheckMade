using CheckMade.Common.Utils;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services;

public interface IBotUpdateHandler
{
    // ToDo: eventually change the return type to the DTO/Model that represents Outputs with all their props.
    Task HandleUpdateAsync(Update update, BotType botType);
}

public class BotUpdateHandler(
        IBotClientFactory botClientFactory,
        IRequestProcessor requestProcessor,
        IToModelConverter converter,
        ILogger<BotUpdateHandler> logger) 
    : IBotUpdateHandler
{
    private const string CallToActionMessageAfterErrorReport = "Please report to your supervisor or contact support.";
    
    public async Task HandleUpdateAsync(Update update, BotType botType)
    {
        logger.LogInformation("Invoke telegram update function for: {botType}", botType);
        
        if (update.Message is not { } telegramInputMessage) 
            throw new InvalidOperationException("Right now, only updates with a 'Message' can be handled.");

        logger.LogInformation("Received Message from {ChatId}", telegramInputMessage.Chat.Id);

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
                DataAccessException => "An error has occured during a data access operation.",
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

        await botClient.SendTextMessageAsync(
            chatId: telegramInputMessage.Chat.Id,
            text: outputMessage);
    }
}
