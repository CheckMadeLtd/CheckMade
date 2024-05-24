using CheckMade.Common.LangExt;
using CheckMade.Common.Utils;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Model;
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

    Task<File> GetFileOrThrowAsync(string fileId);

    Task<Unit> SetBotCommandMenuOrThrowAsync(BotCommandMenus menu, BotType botType);
}

internal class BotClientWrapper(
        ITelegramBotClient botClient,
        INetworkRetryPolicy retryPolicy,
        IUiTranslator translator,
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
            throw new NetworkAccessException(Ui("Failed to reach Telegram servers after several attempts."), ex);
        }
        
        return Unit.Value;
    } 
    
    public async Task<File> GetFileOrThrowAsync(string fileId) => await botClient.GetFileAsync(fileId);

    public async Task<Unit> SetBotCommandMenuOrThrowAsync(BotCommandMenus menu, BotType botType)
    {
        await botClient.DeleteMyCommandsAsync();

        var telegramBotCommands = botType switch
        {
            BotType.Submissions => GetTelegramBotCommandsFromModelCommandsMenu(menu.SubmissionsBotCommandMenu),
            BotType.Communications => GetTelegramBotCommandsFromModelCommandsMenu(menu.CommunicationsBotCommandMenu),
            BotType.Notifications => GetTelegramBotCommandsFromModelCommandsMenu(menu.NotificationsBotCommandMenu),
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };
        
        await botClient.SetMyCommandsAsync(telegramBotCommands);
        
        return Unit.Value;
    }

    private BotCommand[] GetTelegramBotCommandsFromModelCommandsMenu<TEnum>(
        IDictionary<TEnum, ModelBotCommand> menu) where TEnum : Enum =>
        menu
            .Select(kvp => new BotCommand
            {
                Command = translator.Translate(kvp.Value.Command), 
                Description = translator.Translate(kvp.Value.Description)
            }).ToArray();
}

