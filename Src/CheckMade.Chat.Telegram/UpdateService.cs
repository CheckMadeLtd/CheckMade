using CheckMade.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Chat.Telegram;

public class UpdateService(ITelegramBotClient botClient,
    IResponseGenerator responseGenerator,
    ILogger<UpdateService> logger)
{
    internal async Task EchoAsync(Update update)
    {
        logger.LogDebug("Test Debug Log 12ABCD");
        logger.LogWarning("Test Warning Log 12ABCD");
        
        logger.LogInformation("Invoke telegram update function");

        if (update.Message is not { } inputMessage) return;

        logger.LogInformation("Received Message from {ChatId}", inputMessage.Chat.Id);

        var outputMessage = string.Empty;
        
        if (!string.IsNullOrWhiteSpace(inputMessage.Text))
        {
            outputMessage = responseGenerator.Echo(inputMessage.Text);
        }
        
        await botClient.SendTextMessageAsync(
            chatId: inputMessage.Chat.Id,
            text: outputMessage);
        
        throw new ArgumentException("A custom exception test message 12ABCD");
    }
}


