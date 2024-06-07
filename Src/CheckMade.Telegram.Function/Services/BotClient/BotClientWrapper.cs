using CheckMade.Common.Model;
using CheckMade.Common.Model.Telegram.Updates;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Telegram.Model.BotCommand;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = Telegram.Bot.Types.BotCommand;
using File = Telegram.Bot.Types.File;

namespace CheckMade.Telegram.Function.Services.BotClient;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

public interface IBotClientWrapper
{
    BotType MyBotType { get; }
    string MyBotToken { get; }
    
    Task<File> GetFileAsync(string fileId);

    Task<Unit> SendDocumentAsync(
        AttachmentSendOutParameters documentSendOutParams,
        CancellationToken cancellationToken = default);

    Task<Unit> SendLocationAsync(
        ChatId chatId,
        Geo location,
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);
    
    Task<Unit> SendPhotoAsync(
        AttachmentSendOutParameters photoSendOutParams,
        CancellationToken cancellationToken = default);
    
    Task<Unit> SendTextMessageAsync(
        ChatId chatId,
        string pleaseChooseText,
        string text,
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);

    Task<Unit> SendVoiceAsync(
        AttachmentSendOutParameters voiceSendOutParams,
        CancellationToken cancellationToken = default);
    
    Task<Unit> SetBotCommandMenuAsync(BotCommandMenus menu);
}

public class BotClientWrapper(
        ITelegramBotClient botClient,
        INetworkRetryPolicy retryPolicy,
        BotType botType,
        string botToken,
        ILogger<BotClientWrapper> logger) 
    : IBotClientWrapper
{
    public BotType MyBotType { get; } = botType; 
    public string MyBotToken { get; } = botToken;

    public async Task<File> GetFileAsync(string fileId) => await botClient.GetFileAsync(fileId);

    public async Task<Unit> SendDocumentAsync(AttachmentSendOutParameters documentSendOutParams,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async () =>
            
            await botClient.SendDocumentAsync(
                chatId: documentSendOutParams.DestinationChatId,
                document: documentSendOutParams.FileStream,
                caption: documentSendOutParams.Caption.Value,
                replyMarkup: documentSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
            );
        
        return Unit.Value;
    }

    public async Task<Unit> SendLocationAsync(
        ChatId chatId, Geo location, Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async () =>
            
            await botClient.SendLocationAsync(
                chatId: chatId,
                latitude: location.Latitude,
                longitude: location.Longitude,
                replyMarkup: replyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
            );

        return Unit.Value;
    }

    public async Task<Unit> SendPhotoAsync(AttachmentSendOutParameters photoSendOutParams,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async () =>
            
            await botClient.SendPhotoAsync(
                chatId: photoSendOutParams.DestinationChatId,
                photo: photoSendOutParams.FileStream,
                caption: photoSendOutParams.Caption.Value,
                replyMarkup: photoSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
            );
        
        return Unit.Value;
    }

    public async Task<Unit> SendTextMessageAsync(
        ChatId chatId,
        string pleaseChooseText,
        string text,
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default)
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
        
        return Unit.Value;
    }

    public async Task<Unit> SendVoiceAsync(AttachmentSendOutParameters voiceSendOutParams,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async () =>
        
            /* This will throw 'Telegram.Bot.Exceptions.ApiRequestException: Bad Request: VOICE_MESSAGES_FORBIDDEN'
             for Telegram Premium users that in their privacy settings have the default setting that Voice messages
             are only allowed for 'My Contacts'. These exceptions will show up in our Error Logs alongside the User's
             Telegram ID. For now, we need to manually inform them that they need to change their settings to enable
             receiving Voice messages from the Bot (e.g. by adding the Bot to the 'Always Allowed' list). 
             */ 
            await botClient.SendVoiceAsync(
                chatId: voiceSendOutParams.DestinationChatId,
                voice: voiceSendOutParams.FileStream,
                caption: voiceSendOutParams.Caption.Value,
                replyMarkup: voiceSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
            );
        
        return Unit.Value;
    }

    public async Task<Unit> SetBotCommandMenuAsync(BotCommandMenus menu)
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

            await retryPolicy.ExecuteAsync(async () =>
                
                await botClient.SetMyCommandsAsync(
                    telegramBotCommands,
                    scope: null,
                    languageCode: language != LanguageCode.en
                        ? language.ToString()
                        : null) // The English BotCommands are the global default
                ); 

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

