using Telegram.Bot;
using Telegram.Bot.Types;

namespace CheckMade.Telegram.Function.Services;

public interface ITelegramBotClientAdapter
{
    Task<Message> SendTextMessageAsync(
        ChatId chatId,
        string text,
        CancellationToken cancellationToken = default);

}

public class TelegramBotClientAdapter(ITelegramBotClient botClient) : ITelegramBotClientAdapter
{
    public Task<Message> SendTextMessageAsync(
        ChatId chatId, 
        string text,
        CancellationToken cancellationToken = default) => 
            botClient.SendTextMessageAsync(chatId, text, cancellationToken: cancellationToken);
}

