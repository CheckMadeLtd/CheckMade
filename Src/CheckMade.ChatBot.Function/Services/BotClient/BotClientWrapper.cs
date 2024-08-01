using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Utils.RetryPolicies;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = Telegram.Bot.Types.BotCommand;
using File = Telegram.Bot.Types.File;

namespace CheckMade.ChatBot.Function.Services.BotClient;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications, to check that my code 'uses' this important external dependency correctly */

public interface IBotClientWrapper
{
    InteractionMode MyInteractionMode { get; }
    string MyBotToken { get; }
    
    Task<File> GetFileAsync(string fileId);

    Task<Unit> DeleteMessageAsync(
        ChatId chatId,
        int messageId,
        CancellationToken cancellationToken = default);
    
    Task<Unit> EditTextMessageAsync(
        ChatId chatId, 
        Option<string> text,
        int messageId,
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);
    
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

public sealed class BotClientWrapper(
        ITelegramBotClient botClient,
        INetworkRetryPolicy retryPolicy,
        InteractionMode interactionMode,
        string botToken,
        ILogger<BotClientWrapper> logger) 
    : IBotClientWrapper
{
    public InteractionMode MyInteractionMode { get; } = interactionMode; 
    public string MyBotToken { get; } = botToken;

    public async Task<File> GetFileAsync(string fileId) => await botClient.GetFileAsync(fileId);
    
    public async Task<Unit> DeleteMessageAsync(
        ChatId chatId, 
        int messageId, 
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async () =>
            await botClient.DeleteMessageAsync(
                chatId,
                messageId,
                cancellationToken));

        return Unit.Value;
    }

    public async Task<Unit> EditTextMessageAsync(
        ChatId chatId, 
        Option<string> text, 
        int messageId, 
        Option<IReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default)
    {
        var updatedInlineKeyboard = replyMarkup.IsSome
            ? (InlineKeyboardMarkup)replyMarkup.GetValueOrDefault()
            : null;

        if (text.IsSome)
        {
            await retryPolicy.ExecuteAsync(async () =>
                await botClient.EditMessageTextAsync(
                    chatId,
                    messageId,
                    text.GetValueOrThrow(),
                    parseMode: ParseMode.Html,
                    replyMarkup: updatedInlineKeyboard,
                    cancellationToken: cancellationToken));
        }
        else
        {
            await retryPolicy.ExecuteAsync(async () =>
                await botClient.EditMessageReplyMarkupAsync(
                    chatId,
                    messageId,
                    updatedInlineKeyboard,
                    cancellationToken: cancellationToken));
        }

        return Unit.Value;
    }

    public async Task<Unit> SendDocumentAsync(AttachmentSendOutParameters documentSendOutParams,
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async () =>
            await botClient.SendDocumentAsync(
                chatId: documentSendOutParams.ChatId,
                document: documentSendOutParams.FileStream,
                caption: documentSendOutParams.Caption.GetValueOrThrow(),
                parseMode: ParseMode.Html,
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
                chatId: photoSendOutParams.ChatId,
                photo: photoSendOutParams.FileStream,
                caption: photoSendOutParams.Caption.GetValueOrThrow(),
                parseMode: ParseMode.Html,
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
            // Temporarily commented out to avoid redundant 'Please choose...', future review whether really needed or not... 
            
            // if (replyMarkup.GetValueOrDefault() is InlineKeyboardMarkup)
            // {
            //     await botClient.SendTextMessageAsync(
            //         chatId: chatId,
            //         text: pleaseChooseText,
            //         replyMarkup: new ReplyKeyboardRemove(),
            //         cancellationToken: cancellationToken);
            // }

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: replyMarkup.IsSome 
                    ? replyMarkup.GetValueOrThrow()
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
                chatId: voiceSendOutParams.ChatId,
                voice: voiceSendOutParams.FileStream,
                caption: voiceSendOutParams.Caption.GetValueOrThrow(),
                parseMode: ParseMode.MarkdownV2,
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
            var telegramBotCommands = MyInteractionMode switch
            {
                InteractionMode.Operations => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.OperationsBotCommandMenu, language),
                InteractionMode.Communications => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.CommunicationsBotCommandMenu, language),
                InteractionMode.Notifications => 
                    GetTelegramBotCommandsFromModelCommandsMenu(menu.NotificationsBotCommandMenu, language),
                _ => throw new ArgumentOutOfRangeException(nameof(MyInteractionMode))
            };

            await retryPolicy.ExecuteAsync(async () => 
                await botClient.SetMyCommandsAsync(
                    telegramBotCommands,
                    scope: null,
                    languageCode: language != LanguageCode.en
                        ? language.ToString()
                        : null) // The English BotCommands are the global default
            ); 

            logger.LogDebug($"Added to bot {MyInteractionMode} for language {language} " +
                            $"the following BotCommands: " +
                            $"{string.Join("; ", telegramBotCommands.Select(bc => bc.Command))}");
        }
        
        return Unit.Value;
    }

    private static IReadOnlyCollection<BotCommand> GetTelegramBotCommandsFromModelCommandsMenu<TEnum>(
        IReadOnlyDictionary<TEnum, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> menu, LanguageCode language) 
        where TEnum : Enum =>
        menu
            .SelectMany(kvp => kvp.Value)
            .Where(kvp => kvp.Key == language)
            .Select(kvp => new BotCommand
            {
                Command = kvp.Value.Command, 
                Description = kvp.Value.Description
            }).ToImmutableReadOnlyCollection();
}

