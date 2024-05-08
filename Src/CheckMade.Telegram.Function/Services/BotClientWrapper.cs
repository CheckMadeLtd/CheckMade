using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services;

public interface IBotClientWrapper
{
    Task<Message> SendTextMessageAsync(
        ChatId chatId,
        string text,
        CancellationToken cancellationToken = default);

}

internal class BotClientWrapper(ITelegramBotClient botClient) : IBotClientWrapper
{
    public Task<Message> SendTextMessageAsync(
        ChatId chatId, 
        string text,
        CancellationToken cancellationToken = default) => 
            botClient.SendTextMessageAsync(
                chatId,
                text,
                cancellationToken: cancellationToken);
}
