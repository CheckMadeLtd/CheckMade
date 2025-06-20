using CheckMade.Core.Model.Bot.Categories;
using CheckMade.Core.Model.Bot.DTOs;
using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.CrossCutting;
using CheckMade.Core.Model.Common.GIS;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.ExtAPIs.AzureServices;
using CheckMade.Core.ServiceInterfaces.ExtAPIs.Utils;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Bot.Telegram.UpdateHandling;
using General.Utils.FpExtensions.Monads;
using General.Utils.Validators;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Bot.Telegram.Conversion;

public interface IToModelConverter
{
    Task<Result<Input>> ConvertToModelAsync(UpdateWrapper update, InteractionMode interactionMode);
}

public sealed class ToModelConverter(
    ITelegramFilePathResolver filePathResolver,
    IBlobLoader blobLoader,
    IHttpDownloader downloader,
    IAgentRoleBindingsRepository roleBindingsRepo,
    IDomainGlossary domainGlossary,
    ILogger<ToModelConverter> logger) 
    : IToModelConverter
{
    public async Task<Result<Input>> ConvertToModelAsync(UpdateWrapper update, InteractionMode interactionMode)
    {
        return (await
                (from inputType 
                        in GetInputType(update)
                    from attachmentDetails 
                        in GetAttachmentDetails(update)
                    from telegramAttachmentUri
                        in GetTelegramAttachmentUriAsync(attachmentDetails)
                    from internalAttachmentUri
                        in GetInternalAttachmentUriAsync(telegramAttachmentUri)
                    from geoCoordinates 
                        in GetGeoCoordinates(update)
                    from botCommandEnumCode 
                        in GetBotCommandEnumCode(update, interactionMode)
                    from domainTerm 
                        in GetDomainTerm(update, domainGlossary)
                    from controlPromptEnumCode 
                        in GetControlPromptEnumCode(update)
                    from originatorRole
                        in GetOriginatorRoleAsync(update, interactionMode)
                    from liveEventContext
                        in GetLiveEventContext(originatorRole)
                    from input 
                        in GetInput(
                            update, interactionMode, inputType, internalAttachmentUri, attachmentDetails.Type, 
                            geoCoordinates, botCommandEnumCode, domainTerm, controlPromptEnumCode, 
                            originatorRole, liveEventContext) 
                    select input))
            .Match(
                Result<Input>.Succeed,
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

    private static Result<InputType> GetInputType(UpdateWrapper update) =>
        update.Update.Type switch
        {
            UpdateType.Message or UpdateType.EditedMessage => update.Message.Type switch
            {
                MessageType.Text => update.Message.Entities?[0].Type switch
                {
                    MessageEntityType.BotCommand => InputType.CommandMessage,
                    _ => InputType.TextMessage
                },
                MessageType.Location => InputType.Location,
                _ => InputType.AttachmentMessage
            },

            UpdateType.CallbackQuery => InputType.CallbackQuery,

            _ => new InvalidOperationException(
                $"Telegram Update of type {update.Update.Type} is not yet supported " +
                $"and should have never arrived here in {nameof(ConvertToModelAsync)}!")
        };

    private static Result<TelegramAttachmentDetails> GetAttachmentDetails(UpdateWrapper update)
    {
        // No need to check whether we have a valid MessageType, UpdateHandler already filtered that.
        
        // Why Run()? The null-forgiving operators below could throw exceptions if Telegram lib changes! 
        return Result<TelegramAttachmentDetails>.Run(() => update.Message.Type switch
        {
            MessageType.Text or MessageType.Location => new TelegramAttachmentDetails(
                Option<string>.None(), Option<AttachmentType>.None()),

            MessageType.Document => new TelegramAttachmentDetails(
                update.Message.Document!.FileId,
                AttachmentType.Document),

            MessageType.Photo => new TelegramAttachmentDetails(
                update.Message.Photo!.OrderBy(static p => p.FileSize).Last().FileId,
                AttachmentType.Photo),

            MessageType.Voice => new TelegramAttachmentDetails(
                update.Message.Voice!.FileId,
                AttachmentType.Voice),

            _ => throw new ArgumentOutOfRangeException(nameof(GetAttachmentDetails))
        });
    }

    private sealed record TelegramAttachmentDetails(Option<string> FileId, Option<AttachmentType> Type);

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
        
        if (isolatedBotCommand == Start.Command)
            return Option<int>.Some(Start.CommandCode);
        
        var allBotCommandMenus = new BotCommandMenus();

        var botCommandMenuForCurrentMode = interactionMode switch
        {
            InteractionMode.Operations => allBotCommandMenus.OperationsBotCommandMenu.Values,
            InteractionMode.Communications => allBotCommandMenus.CommunicationsBotCommandMenu.Values,
            InteractionMode.Notifications => allBotCommandMenus.NotificationsBotCommandMenu.Values,
            _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
        };
        
        var botCommandFromTelegramUpdate = botCommandMenuForCurrentMode
            .SelectMany(static kvp => kvp.Values)
            .FirstOrDefault(tbc => tbc.Command == isolatedBotCommand);
        
        if (botCommandFromTelegramUpdate == null)
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
                        kvp.Value.Values.Contains(botCommandFromTelegramUpdate))
                    .Key),
            InteractionMode.Communications => Option<int>.Some(
                (int)allBotCommandMenus.CommunicationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromTelegramUpdate))
                    .Key),
            InteractionMode.Notifications => Option<int>.Some(
                (int)allBotCommandMenus.NotificationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromTelegramUpdate))
                    .Key),
            _ => throw new ArgumentOutOfRangeException(nameof(interactionMode))
        };

        return botCommandUnderlyingEnumCodeForModeAgnosticRepresentation;
    }

    private static Result<Option<DomainTerm>> GetDomainTerm(UpdateWrapper update, IDomainGlossary domainGlossary)
    {
        var callBackDataRaw = update.Update.CallbackQuery?.Data;

        if (string.IsNullOrWhiteSpace(callBackDataRaw) || !callBackDataRaw.IsValidToken())
            return Option<DomainTerm>.None();

        return Option<DomainTerm>.Some(domainGlossary.TermById[new CallbackId(callBackDataRaw)]);
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
            .FirstOrDefault(arb =>
                arb.Agent.UserId == update.Message.From?.Id &&
                arb.Agent.ChatId == update.Message.Chat.Id &&
                arb.Agent.Mode == mode)?
            .Role;

        return originatorRole ?? Option<Role>.None();
    }

    private static Result<Option<ILiveEventInfo>> GetLiveEventContext(Option<Role> originatorRole)
    {
        return originatorRole.IsSome 
            ? Option<ILiveEventInfo>.Some(originatorRole.GetValueOrThrow().AtLiveEvent) 
            : Option<ILiveEventInfo>.None();
    }
    
    private static Result<Input> GetInput(
        UpdateWrapper update,
        InteractionMode interactionMode,
        InputType inputType,
        Option<Uri> internalAttachmentUri,
        Option<AttachmentType> attachmentType,
        Option<Geo> geoCoordinates,
        Option<int> botCommandEnumCode,
        Option<DomainTerm> domainTerm,
        Option<long> controlPromptEnumCode,
        Option<Role> originatorRole,
        Option<ILiveEventInfo> liveEventContext)
    {
        if (string.IsNullOrWhiteSpace(update.Message.Text) 
            && internalAttachmentUri.IsNone
            && inputType != InputType.Location)
        {
            return new ArgumentException("A valid Message that is not a Location update must have at least " +
                                         "either a text or an attachment.");   
        }
        
        UserId userId = update.Message.From!.Id; // already checked for non-null in UpdateWrapper constructor
        ChatId chatId = update.Message.Chat.Id;
        
        var messageText = !string.IsNullOrWhiteSpace(update.Message.Text)
            ? update.Message.Text
            : update.Message.Caption;
        
        return new Input(
            update.Message.Date,
            update.Message.MessageId,
            new Agent(userId, chatId, interactionMode), 
            inputType,
            originatorRole.IsSome 
                ? Option<IRoleInfo>.Some(originatorRole.GetValueOrThrow()) 
                : Option<IRoleInfo>.None(),
            liveEventContext,
            Option<ResultantWorkflowState>.None(), 
            Option<Guid>.None(), 
            update.Update.CallbackQuery?.Id ?? Option<string>.None(), 
            new InputDetails(
                !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(), 
                internalAttachmentUri,
                attachmentType,
                geoCoordinates,
                botCommandEnumCode,
                domainTerm,
                controlPromptEnumCode));
    }
    
    private async Task<Result<Option<Uri>>> GetTelegramAttachmentUriAsync(TelegramAttachmentDetails attachmentDetails)
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

    private async Task<Result<Option<Uri>>> GetInternalAttachmentUriAsync(Option<Uri> telegramAttachmentUri)
    {
        return await telegramAttachmentUri.Match(
            UploadBlobAndGetInternalUriAsync,
            static () => Task.FromResult<Result<Option<Uri>>>(Option<Uri>.None()));
    }
    
    private async Task<Result<Option<Uri>>> UploadBlobAndGetInternalUriAsync(Uri telegramAttachmentUri)
    {
        return await Result<Option<Uri>>.RunAsync(async () => 
            await blobLoader.UploadBlobAndReturnUriAsync(
                await downloader.DownloadDataAsync(telegramAttachmentUri), 
                GetFileName(telegramAttachmentUri)));
        
        static string GetFileName(Uri aUri) => aUri.AbsoluteUri.Split('/').Last();
    }
}