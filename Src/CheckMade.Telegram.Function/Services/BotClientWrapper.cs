using Telegram.Bot;
using Telegram.Bot.Types;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Telegram.Function.Services;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

public interface IBotClientWrapper
{
    string BotToken { get; }
    
    Task<Message> SendTextMessageAsync(
        ChatId chatId,
        string text,
        CancellationToken cancellationToken = default);

    Task<File> GetFileAsync(string fileId);
}

internal class BotClientWrapper(ITelegramBotClient botClient, string botToken) : IBotClientWrapper
{
    public string BotToken { get; } = botToken;

    public Task<Message> SendTextMessageAsync(
        ChatId chatId, 
        string text,
        CancellationToken cancellationToken = default) => 
            botClient.SendTextMessageAsync(
                chatId,
                text,
                cancellationToken: cancellationToken);

    public Task<File> GetFileAsync(string fileId) => botClient.GetFileAsync(fileId);
}
