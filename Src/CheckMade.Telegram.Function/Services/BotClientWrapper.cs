using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

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
