using CheckMade.Common.LangExt;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = Telegram.Bot.Types.BotCommand;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Telegram.Function.Services.BotClient;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

public interface IBotClientWrapper
{
    string BotToken { get; }
    
    Task<Unit> SendTextMessageOrThrowAsync(
        ChatId chatId,
        string text,
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);

    Task<File> GetFileOrThrowAsync(string fileId);

    Task<Unit> SetBotCommandMenuOrThrowAsync(BotCommandMenus menu, BotType botType);
}

public class BotClientWrapper(
        ITelegramBotClient botClient,
        INetworkRetryPolicy retryPolicy,
        string botToken,
        ILogger<BotClientWrapper> logger) 
    : IBotClientWrapper
{
    public string BotToken { get; } = botToken;
    
    public async Task<Unit> SendTextMessageOrThrowAsync(
        ChatId chatId,
        string text,
        Option<IReplyMarkup> replyMarkup,
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
                    replyMarkup: replyMarkup.GetValueOrDefault(),
                    cancellationToken: cancellationToken);
            });
        }
        catch (Exception ex)
        {
            throw new NetworkAccessException("Failed to reach Telegram servers after several attempts.", ex);
        }
        
        return Unit.Value;
    }
    
    public async Task<File> GetFileOrThrowAsync(string fileId) => await botClient.GetFileAsync(fileId);

    public async Task<Unit> SetBotCommandMenuOrThrowAsync(BotCommandMenus menu, BotType botType)
    {
        await botClient.DeleteMyCommandsAsync();

        foreach (LanguageCode language in Enum.GetValues(typeof(LanguageCode)))
        {
            var telegramBotCommands = botType switch
            {
                BotType.Submissions => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.SubmissionsBotCommandMenu, language),
                BotType.Communications => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.CommunicationsBotCommandMenu, language),
                BotType.Notifications => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.NotificationsBotCommandMenu, language),
                _ => throw new ArgumentOutOfRangeException(nameof(botType))
            };
        
            await botClient.SetMyCommandsAsync(
                telegramBotCommands, 
                scope: null,
                languageCode: language != LanguageCode.en 
                    ? language.ToString() 
                    : null); // The English BotCommands are the global default
            
            logger.LogDebug($"Added to bot {botType} for language {language} " +
                            $"the following BotCommands: " +
                            $"{string.Join("; ", telegramBotCommands.Select(bc => bc.Command))}");
        }
        
        return Unit.Value;
    }

    private static BotCommand[] GetTelegramBotCommandsFromModelCommandsMenu<TEnum>(
        IReadOnlyDictionary<TEnum, IReadOnlyDictionary<LanguageCode, ModelBotCommand>> menu, LanguageCode language) 
        where TEnum : Enum =>
        menu
            .SelectMany(kvp => kvp.Value)
            .Where(kvp => kvp.Key == language)
            .Select(kvp => new BotCommand
            {
                Command = kvp.Value.Command, 
                Description = kvp.Value.Description
            }).ToArray();
}

