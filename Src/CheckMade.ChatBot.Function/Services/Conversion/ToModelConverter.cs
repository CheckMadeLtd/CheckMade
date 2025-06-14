using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.DomainModel.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.DomainModel.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.ChatBot.UserInteraction.BotCommands;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.LiveEvents;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace CheckMade.ChatBot.Function.Services.Conversion;

public interface IToModelConverter
{
    Task<Result<TlgInput>> ConvertToModelAsync(UpdateWrapper update, InteractionMode interactionMode);
}

internal sealed class ToModelConverter(
    ITelegramFilePathResolver filePathResolver,
    IBlobLoader blobLoader,
    IHttpDownloader downloader,
    ITlgAgentRoleBindingsRepository roleBindingsRepo,
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
                    from tlgAttachmentUri
                        in GetTlgAttachmentUriAsync(attachmentDetails)
                    from internalAttachmentUri
                        in GetInternalAttachmentUriAsync(tlgAttachmentUri)
                    from geoCoordinates 
                        in GetGeoCoordinates(update)
                    from botCommandEnumCode 
                        in GetBotCommandEnumCode(update, interactionMode)
                    from domainTerm 
                        in GetDomainTerm(update)
                    from controlPromptEnumCode 
                        in GetControlPromptEnumCode(update)
                    from originatorRole
                        in GetOriginatorRoleAsync(update, interactionMode)
                    from liveEventContext
                        in GetLiveEventContext(originatorRole)
                    from tlgInput 
                        in GetTlgInput(
                            update, interactionMode, tlgInputType, internalAttachmentUri, attachmentDetails.Type, 
                            geoCoordinates, botCommandEnumCode, domainTerm, controlPromptEnumCode, 
                            originatorRole, liveEventContext) 
                    select tlgInput))
            .Match(
                Result<TlgInput>.Succeed,
                failure =>
                {
                    switch (failure)
                    {
                        case ExceptionWrapper exw:
                            logger.LogError($"""
                                             The following exception occured during an update's conversion to model in method 
                                             '{nameof(ConvertToModelAsync)}': '{exw.Exception.Message}'.
                                             Next, more update details for debugging. InteractionMode: '{interactionMode}'; 
                                             {update.Update.Type}; {update.Message.From?.Id ?? 0}; {update.Message.Chat.Id}; 
                                             {update.Message.Type}; {update.Message.MessageId}; {update.Message.Text ?? ""};  
                                             {update.Message.Date};
                                             """);
                            return exw;
                        
                        default:
                            // The BusinessError message will be converted to user-facing outputs in the InputProcessor.
                            return UiConcatenate(
                                Ui("Failed to convert your Telegram Message: "),
                                ((BusinessError)failure).Error);
                    }
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

            _ => new InvalidOperationException(
                $"Telegram Update of type {update.Update.Type} is not yet supported " +
                $"and should have never arrived here in {nameof(ConvertToModelAsync)}!")
        };

    private static Result<TlgAttachmentDetails> GetAttachmentDetails(UpdateWrapper update)
    {
        if (!IsMessageTypeSupported(update.Message.Type))
        {
            return Ui($"Attachment type {update.Message.Type} is not yet supported!");
        }
        
        // Why Run()? The null-forgiving operators below could throw exceptions if Telegram lib changes! 
        return Result<TlgAttachmentDetails>.Run(() => update.Message.Type switch
        {
            MessageType.Text or MessageType.Location => new TlgAttachmentDetails(
                Option<string>.None(), Option<TlgAttachmentType>.None()),

            MessageType.Document => new TlgAttachmentDetails(
                update.Message.Document!.FileId,
                TlgAttachmentType.Document),

            MessageType.Photo => new TlgAttachmentDetails(
                update.Message.Photo!.OrderBy(static p => p.FileSize).Last().FileId,
                TlgAttachmentType.Photo),

            MessageType.Voice => new TlgAttachmentDetails(
                update.Message.Voice!.FileId,
                TlgAttachmentType.Voice),

            _ => throw new ArgumentOutOfRangeException(nameof(GetAttachmentDetails))
        });

        static bool IsMessageTypeSupported(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Text or MessageType.Location or 
                    MessageType.Document or MessageType.Photo or MessageType.Voice => true,
                _ => false
            };
        }
    }

    private sealed record TlgAttachmentDetails(Option<string> FileId, Option<TlgAttachmentType> Type);

    private static Result<Option<Geo>> GetGeoCoordinates(UpdateWrapper update) =>
        update.Message.Location switch
        {
            { } location => Option<Geo>.Some(new Geo(
                location.Latitude,
                location.Longitude,
                location.HorizontalAccuracy ?? Option<double>.None())),
            
            _ => Option<Geo>.None() 
        };
    
    private static Result<Option<int>> GetBotCommandEnumCode(UpdateWrapper update, InteractionMode interactionMode)
    {
        var botCommandEntity = update.Message.Entities?
            .FirstOrDefault(static e => e.Type == MessageEntityType.BotCommand);

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
            .SelectMany(static kvp => kvp.Values)
            .FirstOrDefault(tbc => tbc.Command == isolatedBotCommand);
        
        if (tlgBotCommandFromTelegramUpdate == null)
            return UiConcatenate(
                Ui("The BotCommand {0} does not exist for the {1}Bot.", 
                    update.Message.Text ?? "[empty text]", interactionMode),
                IInputProcessor.SeeValidBotCommandsInstruction);

        // The botCommandEnumCode will always be interpreted together with the interactionMode. 
        var botCommandUnderlyingEnumCodeForModeAgnosticRepresentation = interactionMode switch
        {
            InteractionMode.Operations => Option<int>.Some(
                (int)allBotCommandMenus.OperationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(tlgBotCommandFromTelegramUpdate))
                    .Key),
            InteractionMode.Communications => Option<int>.Some(
                (int)allBotCommandMenus.CommunicationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(tlgBotCommandFromTelegramUpdate))
                    .Key),
            InteractionMode.Notifications => Option<int>.Some(
                (int)allBotCommandMenus.NotificationsBotCommandMenu
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

        if (string.IsNullOrWhiteSpace(callBackDataRaw) || !callBackDataRaw.IsValidToken())
            return Option<DomainTerm>.None();

        return Option<DomainTerm>.Some(glossary.TermById[new CallbackId(callBackDataRaw)]);
    }
    
    private static Result<Option<long>> GetControlPromptEnumCode(UpdateWrapper update)
    {
        // Only ControlPromptEnumCodes are in format 'long', so if parsable, that's what it is (vs. token for DomainTerm)
        return long.TryParse(update.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData
            : Option<long>.None();
    }
    
    private async Task<Result<Option<Role>>> GetOriginatorRoleAsync(UpdateWrapper update, InteractionMode mode)
    {
        var originatorRole = (await roleBindingsRepo.GetAllActiveAsync())
            .FirstOrDefault(tarb =>
                tarb.TlgAgent.UserId == update.Message.From?.Id &&
                tarb.TlgAgent.ChatId == update.Message.Chat.Id &&
                tarb.TlgAgent.Mode == mode)?
            .Role;

        return originatorRole ?? Option<Role>.None();
    }

    private static Result<Option<ILiveEventInfo>> GetLiveEventContext(Option<Role> originatorRole)
    {
        return originatorRole.IsSome 
            ? Option<ILiveEventInfo>.Some(originatorRole.GetValueOrThrow().AtLiveEvent) 
            : Option<ILiveEventInfo>.None();
    }
    
    private static Result<TlgInput> GetTlgInput(
        UpdateWrapper update,
        InteractionMode interactionMode,
        TlgInputType tlgInputType,
        Option<Uri> internalAttachmentUri,
        Option<TlgAttachmentType> attachmentType,
        Option<Geo> geoCoordinates,
        Option<int> botCommandEnumCode,
        Option<DomainTerm> domainTerm,
        Option<long> controlPromptEnumCode,
        Option<Role> originatorRole,
        Option<ILiveEventInfo> liveEventContext)
    {
        if (string.IsNullOrWhiteSpace(update.Message.Text) 
            && internalAttachmentUri.IsNone
            && tlgInputType != TlgInputType.Location)
        {
            return new ArgumentException("A valid Message that is not a Location update must have at least " +
                                         "either a text or an attachment.");   
        }
        
        TlgUserId userId = update.Message.From!.Id; // already checked for non-null in UpdateWrapper constructor
        TlgChatId chatId = update.Message.Chat.Id;
        
        var messageText = !string.IsNullOrWhiteSpace(update.Message.Text)
            ? update.Message.Text
            : update.Message.Caption;
        
        return new TlgInput(
            update.Message.Date,
            update.Message.MessageId,
            new TlgAgent(userId, chatId, interactionMode), 
            tlgInputType,
            originatorRole.IsSome 
                ? Option<IRoleInfo>.Some(originatorRole.GetValueOrThrow()) 
                : Option<IRoleInfo>.None(),
            liveEventContext,
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            update.Update.CallbackQuery?.Id ?? Option<string>.None(), 
            new TlgInputDetails(
                !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(), 
                internalAttachmentUri,
                attachmentType,
                geoCoordinates,
                botCommandEnumCode,
                domainTerm,
                controlPromptEnumCode));
    }
    
    private async Task<Result<Option<Uri>>> GetTlgAttachmentUriAsync(TlgAttachmentDetails attachmentDetails)
    {
        if (attachmentDetails.FileId.IsNone)
            return Option<Uri>.None();

        return (await GetPathAsync())
            .Match(
                static path => Option<Uri>.Some(GetUriFromPath(path)), 
                Result<Option<Uri>>.Fail
            );
        
        async Task<Result<string>> GetPathAsync() =>
            await filePathResolver.GetTelegramFilePathAsync(attachmentDetails.FileId.GetValueOrThrow());

        static Uri GetUriFromPath(string path) => new(path);
    }

    private async Task<Result<Option<Uri>>> GetInternalAttachmentUriAsync(Option<Uri> tlgAttachmentUri)
    {
        return await tlgAttachmentUri.Match(
            UploadBlobAndGetInternalUriAsync,
            static () => Task.FromResult<Result<Option<Uri>>>(Option<Uri>.None()));
    }
    
    private async Task<Result<Option<Uri>>> UploadBlobAndGetInternalUriAsync(Uri tlgAttachmentUri)
    {
        return await Result<Option<Uri>>.RunAsync(async () => 
            await blobLoader.UploadBlobAndReturnUriAsync(
                await downloader.DownloadDataAsync(tlgAttachmentUri), 
                GetFileName(tlgAttachmentUri)));
        
        static string GetFileName(Uri aUri) => aUri.AbsoluteUri.Split('/').Last();
    }
}