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
    public async Task HandleUpdateAsync(Update update, BotType botType)
    {
        logger.LogInformation("Invoke telegram update function for: {botType}", botType);
        
        if (update.Message is not { } telegramInputMessage) 
            throw new ArgumentNullException(nameof(update), "Message must not be null");

        logger.LogInformation("Received Message from {ChatId}", telegramInputMessage.Chat.Id);

        var inputMessage = converter.ConvertMessage(telegramInputMessage);
        var outputMessage = await requestProcessor.EchoAsync(inputMessage);

        var botClient = botClientFactory.CreateBotClient(botType);

        await botClient.SendTextMessageAsync(
            chatId: telegramInputMessage.Chat.Id,
            text: outputMessage);
    }
}
