using CheckMade.ChatBot.Telegram.UpdateHandling;
using CheckMade.Common.DomainModel.Data.ChatBot;
using CheckMade.Common.DomainModel.Data.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.Data.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.DomainModel.Data.Core.GIS;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Utils.RetryPolicies;
using CheckMade.Common.Utils.UiTranslation;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BotCommand = Telegram.Bot.Types.BotCommand;
using File = Telegram.Bot.Types.TGFile;

namespace CheckMade.ChatBot.Telegram.BotClient;

/* The need for a Wrapper around ITelegramBotClient arises from the need to be able to mock it in unit tests
 and thereby allow verifications to check that my code 'uses' this important external dependency correctly */

public interface IBotClientWrapper
{
    InteractionMode MyInteractionMode { get; }
    string MyBotToken { get; }
    
    Task<File> GetFileAsync(string fileId);

    Task<Unit> DeleteMessageAsync(
        ChatId chatId,
        int messageId,
        CancellationToken cancellationToken = default);
    
    Task<TlgMessageId> EditTextMessageAsync(
        ChatId chatId, 
        Option<string> text,
        int messageId,
        Option<ReplyMarkup> replyMarkup,
        Option<string> callbackQueryId,
        CancellationToken cancellationToken = default);
    
    Task<TlgMessageId> SendDocumentAsync(
        AttachmentSendOutParameters documentSendOutParams,
        CancellationToken cancellationToken = default);

    Task<TlgMessageId> SendLocationAsync(
        ChatId chatId,
        Geo location,
        Option<ReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);
    
    Task<TlgMessageId> SendPhotoAsync(
        AttachmentSendOutParameters photoSendOutParams,
        CancellationToken cancellationToken = default);
    
    Task<TlgMessageId> SendTextMessageAsync(
        ChatId chatId,
        string pleaseChooseText,
        string text,
        Option<ReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default);

    Task<TlgMessageId> SendVoiceAsync(
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

    public async Task<File> GetFileAsync(string fileId) => await botClient.GetFile(fileId);
    
    public async Task<Unit> DeleteMessageAsync(
        ChatId chatId, 
        int messageId, 
        CancellationToken cancellationToken = default)
    {
        await retryPolicy.ExecuteAsync(async () =>
            await botClient.DeleteMessage(
                chatId,
                messageId,
                cancellationToken));

        return Unit.Value;
    }

    public async Task<TlgMessageId> EditTextMessageAsync(
        ChatId chatId, 
        Option<string> text, 
        int messageId, 
        Option<ReplyMarkup> replyMarkup,
        Option<string> callbackQueryId,
        CancellationToken cancellationToken = default)
    {
        Message? editedMessage = null;
        TlgMessageId? newMessageId = null;
        
        // EditMessageX only supports updates to/with InlineKeyboardMarkup!
        if (replyMarkup.GetValueOrDefault() is not ReplyKeyboardMarkup)
        {
            var updatedInlineKeyboard = replyMarkup.IsSome
                ? (InlineKeyboardMarkup)replyMarkup.GetValueOrDefault()
                : null;
            
            try
            {
                // This limits the showing of 'Loading...' on top of Telegram client to the duration of processing
                // rather than a few seconds longer.
                if (callbackQueryId.IsSome)
                {
                    await retryPolicy.ExecuteAsync(async () => 
                        await botClient.AnswerCallbackQuery(
                            callbackQueryId.GetValueOrThrow(),
                            cancellationToken: cancellationToken));
                }
                
                if (text.IsSome)
                {
                    await retryPolicy.ExecuteAsync(async () =>
                        editedMessage = await botClient.EditMessageText(
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
                        editedMessage = await botClient.EditMessageReplyMarkup(
                            chatId,
                            messageId,
                            updatedInlineKeyboard,
                            cancellationToken: cancellationToken));
                }
            }
            catch (ApiRequestException)
            {
                // Gets thrown for "Bad Request: message can't be edited", in which case we delete and update
                editedMessage = null;
                await DeleteAndNewAsync();
            }
        }
        else
        {
            await DeleteAndNewAsync();
        }
        
        if (editedMessage is null && newMessageId is null)
            throw new InvalidOperationException(
                $"{nameof(SendDocumentAsync)} failed to edit (or delete & send new).");
        
        return editedMessage?.MessageId ?? newMessageId!;

        async Task DeleteAndNewAsync()
        {
            await retryPolicy.ExecuteAsync(async () =>
            {
                await DeleteMessageAsync(
                    chatId,
                    messageId,
                    cancellationToken);

                newMessageId = await SendTextMessageAsync(
                    chatId,
                    string.Empty,
                    text.IsSome ? text.GetValueOrThrow() : string.Empty,
                    replyMarkup,
                    cancellationToken);
            });
        }
    }

    public async Task<TlgMessageId> SendDocumentAsync(
        AttachmentSendOutParameters documentSendOutParams,
        CancellationToken cancellationToken = default)
    {
        Message? sentMessage = null;
        
        await retryPolicy.ExecuteAsync(async () =>
            sentMessage = await botClient.SendDocument(
                chatId: documentSendOutParams.ChatId,
                document: documentSendOutParams.FileStream,
                caption: documentSendOutParams.Caption.GetValueOrDefault(),
                parseMode: ParseMode.Html,
                replyMarkup: documentSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
        );
        
        if (sentMessage is null)
            throw new InvalidOperationException(
                $"No {nameof(sentMessage)} was returned by {nameof(SendDocumentAsync)}");
        
        return sentMessage.MessageId;
    }

    public async Task<TlgMessageId> SendLocationAsync(
        ChatId chatId, Geo location, Option<ReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default)
    {
        Message? sentMessage = null;
        
        await retryPolicy.ExecuteAsync(async () =>
            sentMessage = await botClient.SendLocation(
                chatId: chatId,
                latitude: location.Latitude,
                longitude: location.Longitude,
                replyMarkup: replyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
        );

        if (sentMessage is null)
            throw new InvalidOperationException(
                $"No {nameof(sentMessage)} was returned by {nameof(SendLocationAsync)}");
        
        return sentMessage.MessageId;
    }

    public async Task<TlgMessageId> SendPhotoAsync(
        AttachmentSendOutParameters photoSendOutParams,
        CancellationToken cancellationToken = default)
    {
        Message? sentMessage = null;
        
        await retryPolicy.ExecuteAsync(async () =>
            sentMessage = await botClient.SendPhoto(
                chatId: photoSendOutParams.ChatId,
                photo: photoSendOutParams.FileStream,
                caption: photoSendOutParams.Caption.GetValueOrDefault(),
                parseMode: ParseMode.Html,
                replyMarkup: photoSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
        );
        
        if (sentMessage is null)
            throw new InvalidOperationException(
                $"No {nameof(sentMessage)} was returned by {nameof(SendPhotoAsync)}");
        
        return sentMessage.MessageId;
    }

    public async Task<TlgMessageId> SendTextMessageAsync(
        ChatId chatId,
        string pleaseChooseText,
        string text,
        Option<ReplyMarkup> replyMarkup,
        CancellationToken cancellationToken = default)
    {
        Message? sentMessage = null;
        /* Telegram Servers have queues and handle retrying for sending from itself to end user, but this doesn't
        catch earlier network issues in the comms from our Azure Function to the Telegram Servers! */
        await retryPolicy.ExecuteAsync(async () =>
        {
            // This hack is necessary to ensure any previous ReplyKeyboard disappears with any new InlineKeyboard
            // Temporarily commented out to avoid redundant 'Please choose...', future review whether really needed or not... 
            
            // if (replyMarkup.GetValueOrDefault() is InlineKeyboardMarkup)
            // {
            //     sentMessage = await botClient.SendTextMessageAsync(
            //         chatId: chatId,
            //         text: pleaseChooseText,
            //         replyMarkup: new ReplyKeyboardRemove(),
            //         cancellationToken: cancellationToken);
            // }

            sentMessage = await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Html,
                replyMarkup: replyMarkup.IsSome 
                    ? replyMarkup.GetValueOrThrow()
                    : new ReplyKeyboardRemove(), // Ensures removal of previous ReplyKeyboard in all other cases 
                cancellationToken: cancellationToken);
        });

        if (sentMessage is null)
            throw new InvalidOperationException(
                $"No {nameof(sentMessage)} was returned by {nameof(SendTextMessageAsync)}");
        
        return sentMessage.MessageId;
    }

    public async Task<TlgMessageId> SendVoiceAsync(
        AttachmentSendOutParameters voiceSendOutParams,
        CancellationToken cancellationToken = default)
    {
        Message? sentMessage = null;
        
        // See: https://github.com/CheckMadeOrga/CheckMade/issues/197
        await retryPolicy.ExecuteAsync(async () =>
            sentMessage = await botClient.SendVoice(
                chatId: voiceSendOutParams.ChatId,
                voice: voiceSendOutParams.FileStream,
                caption: voiceSendOutParams.Caption.GetValueOrDefault(),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: voiceSendOutParams.ReplyMarkup.GetValueOrDefault(),
                cancellationToken: cancellationToken)
        );
        
        if (sentMessage is null)
            throw new InvalidOperationException(
                $"No {nameof(sentMessage)} was returned by {nameof(SendVoiceAsync)}");
        
        return sentMessage.MessageId;
    }

    public async Task<Unit> SetBotCommandMenuAsync(BotCommandMenus menu)
    {
        await botClient.DeleteMyCommands();

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
                    await botClient.SetMyCommands(
                        telegramBotCommands,
                        scope: null,
                        languageCode: language != LanguageCode.en
                            ? language.ToString()
                            : null) // The English BotCommands are the global default
            ); 

            logger.LogDebug($"Added to bot {MyInteractionMode} for language {language} " +
                            $"the following BotCommands: " +
                            $"{string.Join("; ", telegramBotCommands.Select(static bc => bc.Command))}");
        }
        
        return Unit.Value;
    }

    private static IReadOnlyCollection<BotCommand> GetTelegramBotCommandsFromModelCommandsMenu<TEnum>(
        IReadOnlyDictionary<TEnum, IReadOnlyDictionary<LanguageCode, TlgBotCommand>> menu, LanguageCode language) 
        where TEnum : Enum =>
        menu
            .SelectMany(static kvp => kvp.Value)
            .Where(kvp => kvp.Key == language)
            .Select(static kvp => new BotCommand
            {
                Command = kvp.Value.Command, 
                Description = kvp.Value.Description
            }).ToArray();
}
