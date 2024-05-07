using System.Runtime.CompilerServices;
using CheckMade.Telegram.Interfaces;
using CheckMade.Telegram.Logic;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

[assembly: InternalsVisibleTo("CheckMade.Telegram.Tests")]

namespace CheckMade.Telegram.Function.Services;

public class UpdateHandler(IBotClientFactory botClientFactory,
    IRequestProcessor requestProcessor, IToModelConverter converter, ILogger<UpdateHandler> logger)
{
    internal async Task HandleUpdateAsync(Update update, BotType botType)
    {
        logger.LogInformation("Invoke telegram update function for: {botType}", botType);
        
        if (update.Message is not { } telegramInputMessage) return;

        logger.LogInformation("Received Message from {ChatId}", telegramInputMessage.Chat.Id);

        var inputMessage = converter.ConvertMessage(telegramInputMessage);
        var outputMessage = await requestProcessor.EchoAsync(inputMessage);

        var botClient = botClientFactory.CreateBotClient(botType);

        await botClient.SendTextMessageAsync(
            chatId: telegramInputMessage.Chat.Id,
            text: outputMessage);
    }
}
