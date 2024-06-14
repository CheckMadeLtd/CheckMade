using CheckMade.ChatBot.Function.Services.UpdateHandling;
using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Telegram;
using CheckMade.Common.Model.Telegram.Input;
using CheckMade.Common.Model.Telegram.UserInteraction;
using CheckMade.Common.Model.Telegram.UserInteraction.BotCommands;
using CheckMade.Common.Model.Utils;
using CheckMade.ChatBot.Logic;
using Telegram.Bot.Types.Enums;

namespace CheckMade.ChatBot.Function.Services.Conversion;

public interface IToModelConverter
{
    Task<Result<TlgInput>> ConvertToModelAsync(UpdateWrapper update, InteractionMode interactionMode);
}

internal class ToModelConverter(
        ITelegramFilePathResolver filePathResolver,
        IBlobLoader blobLoader,
        IHttpDownloader downloader) 
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
                    from domainCategoryEnumCode 
                        in GetDomainCategoryEnumCode(update)
                    from controlPromptEnumCode 
                        in GetControlPromptEnumCode(update)
                    from tlgInput 
                        in GetTlgInputAsync(
                            update, interactionMode, tlgInputType, attachmentDetails, geoCoordinates, 
                            botCommandEnumCode, domainCategoryEnumCode, controlPromptEnumCode) 
                    select tlgInput))
            .Match(
                Result<TlgInput>.FromSuccess,
                error => UiConcatenate(
                    Ui("Failed to convert your Telegram Message: "),
                    error)
            );
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

        if (update.Message.Text == TlgStart.Command)
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
            .FirstOrDefault(mbc => mbc.Command == update.Message.Text);
        
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

    private static Result<Option<int>> GetDomainCategoryEnumCode(UpdateWrapper update)
    {
        return int.TryParse(update.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData <= EnumCallbackId.DomainCategoryMaxThreshold
                ? callBackData
                : Option<int>.None()
            : Option<int>.None();
    }
    
    private static Result<Option<long>> GetControlPromptEnumCode(UpdateWrapper update)
    {
        return long.TryParse(update.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData > EnumCallbackId.DomainCategoryMaxThreshold
                ? callBackData
                : Option<long>.None()
            : Option<long>.None();
    }
    
    private async Task<Result<TlgInput>> GetTlgInputAsync(
        UpdateWrapper update,
        InteractionMode interactionMode,
        TlgInputType tlgInputType,
        AttachmentDetails attachmentDetails,
        Option<Geo> geoCoordinates,
        Option<int> botCommandEnumCode,
        Option<int> domainCategoryEnumCode,
        Option<long> controlPromptEnumCode)
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

        var telegramAttachmentUriAttempt = await attachmentDetails.FileId.Match(
            GetTelegramAttachmentUriAsync,
            () => Task.FromResult<Attempt<Option<Uri>>>(Option<Uri>.None()));

        var telegramAttachmentUri = telegramAttachmentUriAttempt.Match(
            uri => uri,
            ex => throw ex);
        
        var internalAttachmentUriAttempt = await telegramAttachmentUri.Match(
            UploadBlobAndGetInternalUriAsync,
            () => Task.FromResult<Attempt<Option<Uri>>>(Option<Uri>.None()));
        
        var internalAttachmentUri = internalAttachmentUriAttempt.Match(
            uri => uri,
            ex => throw ex);
        
        var messageText = !string.IsNullOrWhiteSpace(update.Message.Text)
            ? update.Message.Text
            : update.Message.Caption;
        
        return new TlgInput(userId, chatId, interactionMode, tlgInputType,
            new TlgInputDetails(
                update.Message.Date,
                update.Message.MessageId,
                !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(), 
                telegramAttachmentUri,
                internalAttachmentUri,
                attachmentDetails.Type,
                geoCoordinates,
                botCommandEnumCode,
                domainCategoryEnumCode,
                controlPromptEnumCode));
    }

    private async Task<Attempt<Option<Uri>>> GetTelegramAttachmentUriAsync(string fileId)
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

    private async Task<Attempt<Option<Uri>>> UploadBlobAndGetInternalUriAsync(Uri telegramAttachmentUri)
    {
        return await Attempt<Option<Uri>>.RunAsync(async () => 
            await blobLoader.UploadBlobAndReturnUriAsync(
                await downloader.DownloadDataAsync(telegramAttachmentUri), 
                GetFileName(telegramAttachmentUri)));
        
        static string GetFileName(Uri aUri) => aUri.AbsoluteUri.Split('/').Last();
    }
}
