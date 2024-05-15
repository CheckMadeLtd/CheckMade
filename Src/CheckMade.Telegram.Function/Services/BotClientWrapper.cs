using CheckMade.Common.LanguageExtensions;
using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Telegram.Function.Services;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

public interface IBotClientWrapper
{
    string BotToken { get; }
    
    Task<Unit> SendTextMessageAsync(
        ChatId chatId,
        string text,
        CancellationToken cancellationToken = default);

    Task<File> GetFileAsync(string fileId);
}

internal class BotClientWrapper(
        ITelegramBotClient botClient,
        INetworkRetryPolicy retryPolicy,
        string botToken) 
    : IBotClientWrapper
{
    public string BotToken { get; } = botToken;
    
    public async Task<Unit> SendTextMessageAsync(
        ChatId chatId,
        string text,
        CancellationToken cancellationToken = default)
    {
        try
        {
            /* Telegram Servers have queues and handle retrying for sending from itself to end user, but this doesn't
            catch earlier network issues like from our Azure Function to the Telegram Servers! */
            await retryPolicy.ExecuteAsync(async () =>
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    cancellationToken: cancellationToken);
            });
        }
        catch (Exception ex)
        {
            throw new NetworkAccessException("Failed to reach Telegram servers.", ex);
        }
        
        return Unit.Value;
    } 
    
    public Task<File> GetFileAsync(string fileId) => botClient.GetFileAsync(fileId);
}
