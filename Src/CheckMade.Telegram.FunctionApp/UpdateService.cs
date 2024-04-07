using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.FunctionApp;

public class UpdateService(ITelegramBotClient botClient, ILogger<UpdateService> logger)
{
    internal async Task EchoAsync(Update update)
    {
        logger.LogInformation("Invoke telegram update function");

        if (!(update.Message is { } message)) return;

        logger.LogInformation("Received Message from {ChatId}", message.Chat.Id);
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: $"Echo : {message.Text}");
    }
}