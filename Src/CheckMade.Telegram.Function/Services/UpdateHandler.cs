using System.Runtime.CompilerServices;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using CheckMade.Telegram.Model;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

[assembly: InternalsVisibleTo("CheckMade.Telegram.Tests")]

namespace CheckMade.Telegram.Function.Services;

public class UpdateHandler(IBotClientFactory botClientFactory,
    IRequestProcessor requestProcessor, ILogger<UpdateHandler> logger)
{
    internal async Task HandleUpdateAsync(Update update, BotType botType)
    {
        logger.LogInformation("Invoke telegram update function for: {botType}", botType);
        
        if (update.Message is not { } telegramInputMessage) return;

        logger.LogInformation("Received Message from {ChatId}", telegramInputMessage.Chat.Id);

        var inputMessage = ConvertToModel(telegramInputMessage);
        var outputMessage = await requestProcessor.EchoAsync(inputMessage);

        var botClient = botClientFactory.CreateBotClient(botType);

        await botClient.SendTextMessageAsync(
            chatId: telegramInputMessage.Chat.Id,
            text: outputMessage);
    }

    internal static InputTextMessage ConvertToModel(Message telegramInputMessage)
    {
        var userId = telegramInputMessage.From?.Id 
                     ?? throw new ArgumentNullException(nameof(telegramInputMessage),
                         "From.Id in the input message must not be null");

        var messageText = string.IsNullOrWhiteSpace(telegramInputMessage.Text)
            ? throw new ArgumentNullException(nameof(telegramInputMessage),
                "Text in the telegram input message must not be empty")
            : telegramInputMessage.Text;
        
        return new InputTextMessage(
            userId,
            new MessageDetails(
                messageText,
                telegramInputMessage.Date));
    }
}
