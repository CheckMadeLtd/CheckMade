using CheckMade.Chat.Logic;
using CheckMade.Common.Interfaces;
using CheckMade.Telegram.Function.Startup;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function;

public class UpdateService(ITelegramBotClientFactory botClientFactory,
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
            outputMessage = requestProcessor.Echo(inputMessage.Chat.Id, inputMessage.Text);
        }

        var botClient = botClientFactory.CreateBotClient(botType);

        await botClient.SendTextMessageAsync(
            chatId: inputMessage.Chat.Id,
            text: outputMessage);
    }
}
