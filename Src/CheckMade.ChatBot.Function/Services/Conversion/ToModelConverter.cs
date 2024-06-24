using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace CheckMade.ChatBot.Function.Services.Conversion;

public interface IToModelConverter
{
    Task<Result<TlgInput>> ConvertToModelAsync(UpdateWrapper update, InteractionMode interactionMode);
}

internal class ToModelConverter(
        ITelegramFilePathResolver filePathResolver,
        IBlobLoader blobLoader,
        IHttpDownloader downloader,
        ILogger<ToModelConverter> logger) 
    : IToModelConverter
{
    public async Task<Result<TlgInput>> ConvertToModelAsync(UpdateWrapper update, InteractionMode interactionMode)
    {
        return (await
                (from tlgInputType 
                        in GetTlgInputType(update)
                    from attachmentDetails 
                        in GetAttachmentDetails(update)
                    from geoCoordinates 
                        in GetGeoCoordinates(update)
                    from botCommandEnumCode 
                        in GetBotCommandEnumCode(update, interactionMode)
                    from domainTerm 
                        in GetDomainTerm(update)
                    from controlPromptEnumCode 
                        in GetControlPromptEnumCode(update)
                    from originatorRole
                        in GetOriginatorRole(update)
                    from liveEventContext
                        in GetLiveEventContext(originatorRole)
                    from tlgInput 
                        in GetTlgInputAsync(
                            update, interactionMode, tlgInputType, attachmentDetails, geoCoordinates, 
                            botCommandEnumCode, domainTerm, controlPromptEnumCode, originatorRole, liveEventContext) 
                    select tlgInput))
            .Match(
                Result<TlgInput>.FromSuccess,
                error =>
                {
                    logger.LogWarning($"""
                                       The following error occured during an update's conversion to model in method 
                                       '{nameof(ConvertToModelAsync)}': '{error}'.
                                       Next, more update details for debugging. InteractionMode: '{interactionMode}'; 
                                       {update.Update.Type}; {update.Message.From?.Id ?? 0}; {update.Message.Chat.Id}; 
                                       {update.Message.Type}; {update.Message.MessageId}; {update.Message.Text ?? ""};  
                                       {update.Message.Date};
                                       """);
                    
                    return UiConcatenate(
                        Ui("Failed to convert your Telegram Message: "),
                        error);
                });
    }

    private static Result<TlgInputType> GetTlgInputType(UpdateWrapper update) =>
        update.Update.Type switch
        {
            UpdateType.Message or UpdateType.EditedMessage => update.Message.Type switch
            {
                MessageType.Text => update.Message.Entities?[0].Type switch
                {
                    MessageEntityType.BotCommand => TlgInputType.CommandMessage,
                    _ => TlgInputType.TextMessage
                },
                MessageType.Location => TlgInputType.Location,
                _ => TlgInputType.AttachmentMessage
            },

            UpdateType.CallbackQuery => TlgInputType.CallbackQuery,

            _ => throw new InvalidOperationException(
                $"Telegram Update of type {update.Update.Type} is not yet supported " +
                $"and shouldn't be handled in this converter!")
        };

    private static Result<AttachmentDetails> GetAttachmentDetails(UpdateWrapper update)
    {
        // These stay proper Exceptions b/c they'd represent totally unexpected behaviour from an external library!
        const string errorMessage = "For Telegram message of type {0} we expect the {0} property to not be null";

        return update.Message.Type switch
        {
            MessageType.Text or MessageType.Location => new AttachmentDetails(
                Option<string>.None(), Option<TlgAttachmentType>.None()),

            MessageType.Document => new AttachmentDetails(
                update.Message.Document?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, update.Message.Type)),
                TlgAttachmentType.Document),

            MessageType.Photo => new AttachmentDetails(
                update.Message.Photo?.OrderBy(p => p.FileSize).Last().FileId
                ?? throw new InvalidOperationException(
                    string.Format(errorMessage, update.Message.Type)),
                TlgAttachmentType.Photo),

            MessageType.Voice => new AttachmentDetails(
                update.Message.Voice?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, update.Message.Type)),
                TlgAttachmentType.Voice),

            _ => Ui("Attachment type {0} is not yet supported!", update.Message.Type)
        };
    }

    private record AttachmentDetails(Option<string> FileId, Option<TlgAttachmentType> Type);

    private static Result<Option<Geo>> GetGeoCoordinates(UpdateWrapper update) =>
        update.Message.Location switch
        {
            { } location => Option<Geo>.Some(new Geo(
                location.Latitude,
                location.Longitude,
                location.HorizontalAccuracy ?? Option<float>.None())),
            
            _ => Option<Geo>.None() 
        };
    
    private static Result<Option<int>> GetBotCommandEnumCode(UpdateWrapper update, InteractionMode interactionMode)
    {
        var botCommandEntity = update.Message.Entities?
            .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

        if (botCommandEntity == null)
            return Option<int>.None();

        // In Telegram Group Chats, any botCommand is appended with @[botName]
        var isolatedBotCommand = update.Message.Text!.Split('@')[0];
        
        if (isolatedBotCommand == TlgStart.Command)
            return Option<int>.Some(TlgStart.CommandCode);
        
        var allBotCommandMenus = new BotCommandMenus();

        var botCommandMenuForCurrentMode = interactionMode switch
        {
            InteractionMode.Operations => allBotCommandMenus.OperationsBotCommandMenu.Values,
            InteractionMode.Communications => allBotCommandMenus.CommunicationsBotCommandMenu.Values,
            InteractionMode.Notifications => allBotCommandMenus.NotificationsBotCommandMenu.Values,
            _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
        };
        
        var tlgBotCommandFromTelegramUpdate = botCommandMenuForCurrentMode
            .SelectMany(kvp => kvp.Values)
            .FirstOrDefault(mbc => mbc.Command == isolatedBotCommand);
        
        if (tlgBotCommandFromTelegramUpdate == null)
            return UiConcatenate(
                Ui("The BotCommand {0} does not exist for the {1}Bot [errcode: {2}]. ", 
                    update.Message.Text ?? "[empty text!]", interactionMode, "W3DL9"),
                IInputProcessor.SeeValidBotCommandsInstruction);

        var botCommandUnderlyingEnumCodeForModeAgnosticRepresentation = interactionMode switch
        {
            InteractionMode.Operations => Option<int>.Some(
                (int) allBotCommandMenus.OperationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(tlgBotCommandFromTelegramUpdate))
                    .Key),
            InteractionMode.Communications => Option<int>.Some(
                (int) allBotCommandMenus.CommunicationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(tlgBotCommandFromTelegramUpdate))
                    .Key),
            InteractionMode.Notifications => Option<int>.Some(
                (int) allBotCommandMenus.NotificationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(tlgBotCommandFromTelegramUpdate))
                    .Key),
            _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
        };

        return botCommandUnderlyingEnumCodeForModeAgnosticRepresentation;
    }

    private static Result<Option<DomainTerm>> GetDomainTerm(UpdateWrapper update)
    {
        var glossary = new DomainGlossary();
        var callBackDataRaw = update.Update.CallbackQuery?.Data;

        if (string.IsNullOrWhiteSpace(callBackDataRaw))
            return Option<DomainTerm>.None();

        return long.TryParse(update.Update.CallbackQuery?.Data, out _) 
            ? Option<DomainTerm>.None() // This means, it's a ControlPrompt, see below
            : Option<DomainTerm>.Some(glossary.TermById[new CallbackId(callBackDataRaw)]);
    }
    
    private static Result<Option<long>> GetControlPromptEnumCode(UpdateWrapper update)
    {
        return long.TryParse(update.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData
            : Option<long>.None();
    }

    private static Result<Option<IRoleInfo>> GetOriginatorRole(UpdateWrapper update)
    {
        throw new NotImplementedException();
    }

    private static Result<Option<ILiveEventInfo>> GetLiveEventContext(Option<IRoleInfo> originatorRole)
    {
        throw new NotImplementedException();
    }
    
    private async Task<Result<TlgInput>> GetTlgInputAsync(
        UpdateWrapper update,
        InteractionMode interactionMode,
        TlgInputType tlgInputType,
        AttachmentDetails attachmentDetails,
        Option<Geo> geoCoordinates,
        Option<int> botCommandEnumCode,
        Option<DomainTerm> domainTerm,
        Option<long> controlPromptEnumCode,
        Option<IRoleInfo> originatorRole,
        Option<ILiveEventInfo> liveEventContext)
    {
        if (update.Message.From?.Id == null || 
            string.IsNullOrWhiteSpace(update.Message.Text) 
            && attachmentDetails.FileId.IsNone
            && tlgInputType != TlgInputType.Location)
        {
            return Ui("""
                      A valid message must:  
                      a) have a User Id ('From.Id' in Telegram); 
                      b) either have a text or an attachment (unless it's a Location).
                      """);   
        }
        
        TlgUserId userId = update.Message.From.Id;
        TlgChatId chatId = update.Message.Chat.Id;

        var tlgAttachmentUriAttempt = await attachmentDetails.FileId.Match(
            GetTlgAttachmentUriAsync,
            () => Task.FromResult<Attempt<Option<Uri>>>(Option<Uri>.None()));

        var tlgAttachmentUri = tlgAttachmentUriAttempt.Match(
            uri => uri,
            ex => throw ex);
        
        var internalAttachmentUriAttempt = await tlgAttachmentUri.Match(
            UploadBlobAndGetInternalUriAsync,
            () => Task.FromResult<Attempt<Option<Uri>>>(Option<Uri>.None()));
        
        var internalAttachmentUri = internalAttachmentUriAttempt.Match(
            uri => uri,
            ex => throw ex);
        
        var messageText = !string.IsNullOrWhiteSpace(update.Message.Text)
            ? update.Message.Text
            : update.Message.Caption;
        
        return new TlgInput(
            new TlgAgent(userId, chatId, interactionMode), 
            tlgInputType,
            originatorRole,
            liveEventContext,
            new TlgInputDetails(
                update.Message.Date,
                update.Message.MessageId,
                !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(), 
                tlgAttachmentUri,
                internalAttachmentUri,
                attachmentDetails.Type,
                geoCoordinates,
                botCommandEnumCode,
                domainTerm,
                controlPromptEnumCode));
    }

    private async Task<Attempt<Option<Uri>>> GetTlgAttachmentUriAsync(string fileId)
    {
        return (await GetPathAsync())
            .Match(
                path => Option<Uri>.Some(GetUriFromPath(path)), 
                Attempt<Option<Uri>>.Fail
        );
        
        async Task<Attempt<string>> GetPathAsync() =>
            await filePathResolver.GetTelegramFilePathAsync(fileId);

        static Uri GetUriFromPath(string path) => new(path);
    }

    private async Task<Attempt<Option<Uri>>> UploadBlobAndGetInternalUriAsync(Uri tlgAttachmentUri)
    {
        return await Attempt<Option<Uri>>.RunAsync(async () => 
            await blobLoader.UploadBlobAndReturnUriAsync(
                await downloader.DownloadDataAsync(tlgAttachmentUri), 
                GetFileName(tlgAttachmentUri)));
        
        static string GetFileName(Uri aUri) => aUri.AbsoluteUri.Split('/').Last();
    }
}
