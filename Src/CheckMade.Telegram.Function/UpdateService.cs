using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Function.Startup;
using CheckMade.Telegram.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function;

public class UpdateService(IBotClientFactory botClientFactory,
    IRequestProcessor requestProcessor, ILogger<UpdateService> logger)
{
    internal async Task HandleUpdateAsync(Update update, BotType botType)
    {
        logger.LogInformation("Invoke telegram update function for: {botType}", botType);
        if (update.Message is not { } inputMessage) return;
        logger.LogInformation("Received Message from {ChatId}", inputMessage.Chat.Id);

        var outputMessage = string.Empty;
        
        if (!string.IsNullOrWhiteSpace(inputMessage.Text))
        {
            outputMessage = requestProcessor.Echo(inputMessage);
        }

        var botClient = botClientFactory.CreateBotClient(botType);

        await botClient.SendTextMessageAsync(
            chatId: inputMessage.Chat.Id,
            text: outputMessage);
    }
}
