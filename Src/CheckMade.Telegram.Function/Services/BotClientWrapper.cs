using CheckMade.Common.FpExt;
using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Model.BotCommands;
using Telegram.Bot;
using Telegram.Bot.Types;
using BotCommand = Telegram.Bot.Types.BotCommand;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Telegram.Function.Services;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

public interface IBotClientWrapper
{
    string BotToken { get; }
    
    Task<Unit> SendTextMessageOrThrowAsync(
        ChatId chatId,
        string text,
        CancellationToken cancellationToken = default);

    Task<File> GetFileAsync(string fileId);

    Task SetBotCommandMenuOrThrow(SubmissionsBotCommandMenu modelBotCommandMenu);
}

internal class BotClientWrapper(
        ITelegramBotClient botClient,
        INetworkRetryPolicy retryPolicy,
        string botToken) 
    : IBotClientWrapper
{
    public string BotToken { get; } = botToken;
    
    public async Task<Unit> SendTextMessageOrThrowAsync(
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
            throw new NetworkAccessException("Failed to reach Telegram servers after several attempts.", ex);
        }
        
        return Unit.Value;
    } 
    
    // ToDo: turn this into GetFileAsyncOrThrow for consistency and see where this is used and whether it needs
    // wrapping in Attempt<> too
    public async Task<File> GetFileAsync(string fileId) => await botClient.GetFileAsync(fileId);

    // ToDo: Change argument to IBotCommandMenu after introducing it, so that it works for all botTypes
    public async Task SetBotCommandMenuOrThrow(SubmissionsBotCommandMenu modelBotCommandMenu)
    {
        await botClient.DeleteMyCommandsAsync();
        
        await botClient.SetMyCommandsAsync(modelBotCommandMenu.Menu
            .Select(kvp => new
            {
                ModelCommand = kvp.Value.Command,
                ModelDescription = kvp.Value.Description
            })
            .Select(pair => new BotCommand
            {
                Command = pair.ModelCommand,
                Description = pair.ModelDescription
            })
            .ToArray());
    }
}

