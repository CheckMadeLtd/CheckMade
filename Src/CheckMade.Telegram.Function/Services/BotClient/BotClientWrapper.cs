using CheckMade.Common.LangExt;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Model.BotCommand;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = Telegram.Bot.Types.BotCommand;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Telegram.Function.Services.BotClient;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

// ToDo: Wrap all calls to Send in retry policy, like for Text (after done debugging exception and voice behaviour)

public interface IBotClientWrapper
{
    BotType MyBotType { get; }
    string MyBotToken { get; }
    
    Task<File> GetFileOrThrowAsync(string fileId);

    Task<Unit> SendDocumentOrThrowAsync(
        AttachmentSendOutParameters documentSendOutParams,
        CancellationToken cancellationToken = default);

    Task<Unit> SendLocationOrThrowAsync(
        ChatId chatId,
        Geo location,
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);
    
    Task<Unit> SendPhotoOrThrowAsync(
        AttachmentSendOutParameters photoSendOutParams,
        CancellationToken cancellationToken = default);
    
    Task<Unit> SendTextMessageOrThrowAsync(
        ChatId chatId,
        string pleaseChooseText,
        string text,
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);

    Task<Unit> SendVoiceOrThrowAsync(
        AttachmentSendOutParameters voiceSendOutParams,
        CancellationToken cancellationToken = default);
    
    Task<Unit> SetBotCommandMenuOrThrowAsync(BotCommandMenus menu);
}

public class BotClientWrapper(
        ITelegramBotClient botClient,
        INetworkRetryPolicy retryPolicy,
        BotType botType,
        string botToken,
        ILogger<BotClientWrapper> logger) 
    : IBotClientWrapper
{
    private const string TelegramSendOutExceptionMessage =
        "Either failed to construct valid SendOut parameters for " +
        "Telegram or failed to reach its servers (after several attempts).";
    
    public BotType MyBotType { get; } = botType; 
    public string MyBotToken { get; } = botToken;

    public async Task<File> GetFileOrThrowAsync(string fileId) => await botClient.GetFileAsync(fileId);

    public async Task<Unit> SendDocumentOrThrowAsync(AttachmentSendOutParameters documentSendOutParams,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendDocumentAsync(
                chatId: documentSendOutParams.DestinationChatId,
                document: documentSendOutParams.FileStream,
                caption: documentSendOutParams.Caption.Value,
                replyMarkup: documentSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new TelegramSendOutException(TelegramSendOutExceptionMessage, ex);
        }
        
        return Unit.Value;
    }

    public async Task<Unit> SendLocationOrThrowAsync(
        ChatId chatId, Geo location, Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendLocationAsync(
                chatId: chatId,
                latitude: location.Latitude,
                longitude: location.Longitude,
                replyMarkup: replyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new TelegramSendOutException(TelegramSendOutExceptionMessage, ex);
        }

        return Unit.Value;
    }

    public async Task<Unit> SendPhotoOrThrowAsync(AttachmentSendOutParameters photoSendOutParams,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendPhotoAsync(
                chatId: photoSendOutParams.DestinationChatId,
                photo: photoSendOutParams.FileStream,
                caption: photoSendOutParams.Caption.Value,
                replyMarkup: photoSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new TelegramSendOutException(TelegramSendOutExceptionMessage, ex);
        }
        
        return Unit.Value;
    }

    public async Task<Unit> SendTextMessageOrThrowAsync(
        ChatId chatId,
        string pleaseChooseText,
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
                // This hack is necessary to ensure any previous ReplyKeyboard disappears with any new InlineKeyboard
                if (replyMarkup.GetValueOrDefault() is InlineKeyboardMarkup)
                {
                    await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: pleaseChooseText,
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                }
                
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: text,
                    replyMarkup: replyMarkup.IsSome 
                        ? replyMarkup.GetValueOrDefault()
                        : new ReplyKeyboardRemove(), // Ensures removal of previous ReplyKeyboard in all other cases 
                    cancellationToken: cancellationToken);
            });
        }
        catch (Exception ex)
        {
            throw new TelegramSendOutException(TelegramSendOutExceptionMessage, ex);
        }
        
        return Unit.Value;
    }

    public async Task<Unit> SendVoiceOrThrowAsync(AttachmentSendOutParameters voiceSendOutParams,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await botClient.SendVoiceAsync(
                chatId: voiceSendOutParams.DestinationChatId,
                voice: voiceSendOutParams.FileStream,
                caption: voiceSendOutParams.Caption.Value,
                replyMarkup: voiceSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new TelegramSendOutException(TelegramSendOutExceptionMessage, ex);
        }
        
        return Unit.Value;
    }

    public async Task<Unit> SetBotCommandMenuOrThrowAsync(BotCommandMenus menu)
    {
        await botClient.DeleteMyCommandsAsync();

        foreach (LanguageCode language in Enum.GetValues(typeof(LanguageCode)))
        {
            var telegramBotCommands = MyBotType switch
            {
                BotType.Operations => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.OperationsBotCommandMenu, language),
                BotType.Communications => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.CommunicationsBotCommandMenu, language),
                BotType.Notifications => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.NotificationsBotCommandMenu, language),
                _ => throw new ArgumentOutOfRangeException(nameof(MyBotType))
            };
        
            await botClient.SetMyCommandsAsync(
                telegramBotCommands, 
                scope: null,
                languageCode: language != LanguageCode.en 
                    ? language.ToString() 
                    : null); // The English BotCommands are the global default
            
            logger.LogDebug($"Added to bot {MyBotType} for language {language} " +
                            $"the following BotCommands: " +
                            $"{string.Join("; ", telegramBotCommands.Select(bc => bc.Command))}");
        }
        
        return Unit.Value;
    }

    private static BotCommand[] GetTelegramBotCommandsFromModelCommandsMenu<TEnum>(
        IReadOnlyDictionary<TEnum, IReadOnlyDictionary<LanguageCode, TelegramBotCommand>> menu, LanguageCode language) 
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

