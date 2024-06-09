using CheckMade.Common.ExternalServices.ExternalUtils;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Enums;
using CheckMade.Common.Model.Tlg.Input;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Logic.UpdateProcessors;
using CheckMade.Telegram.Model.BotCommand;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services.Conversion;

public interface IToModelConverter
{
    Task<Result<TlgUpdate>> ConvertToModelAsync(UpdateWrapper wrappedUpdate, TlgBotType botType);
}

internal class ToModelConverter(
        ITelegramFilePathResolver filePathResolver,
        IBlobLoader blobLoader,
        IHttpDownloader downloader) 
    : IToModelConverter
{
    public async Task<Result<TlgUpdate>> ConvertToModelAsync(UpdateWrapper wrappedUpdate, TlgBotType botType)
    {
        return (await
                (from modelUpdateType 
                        in GetModelUpdateType(wrappedUpdate)
                    from attachmentDetails 
                        in GetAttachmentDetails(wrappedUpdate)
                    from geoCoordinates 
                        in GetGeoCoordinates(wrappedUpdate)
                    from botCommandEnumCode 
                        in GetBotCommandEnumCode(wrappedUpdate, botType)
                    from domainCategoryEnumCode 
                        in GetDomainCategoryEnumCode(wrappedUpdate)
                    from controlPromptEnumCode 
                        in GetControlPromptEnumCode(wrappedUpdate)
                    from telegramUpdate 
                        in GetTelegramUpdateAsync(
                            wrappedUpdate, botType, modelUpdateType, attachmentDetails, geoCoordinates, 
                            botCommandEnumCode, domainCategoryEnumCode, controlPromptEnumCode) 
                    select telegramUpdate))
            .Match(
                Result<TlgUpdate>.FromSuccess,
                error => UiConcatenate(
                    Ui("Failed to convert your Telegram Message: "),
                    error)
            );
    }

    private static Result<TlgUpdateType> GetModelUpdateType(UpdateWrapper wrappedUpdate) =>
        wrappedUpdate.Update.Type switch
        {
            UpdateType.Message or UpdateType.EditedMessage => wrappedUpdate.Message.Type switch
            {
                MessageType.Text => wrappedUpdate.Message.Entities?[0].Type switch
                {
                    MessageEntityType.BotCommand => TlgUpdateType.CommandMessage,
                    _ => TlgUpdateType.TextMessage
                },
                MessageType.Location => TlgUpdateType.Location,
                _ => TlgUpdateType.AttachmentMessage
            },

            UpdateType.CallbackQuery => TlgUpdateType.CallbackQuery,

            _ => throw new InvalidOperationException(
                $"Telegram Update of type {wrappedUpdate.Update.Type} is not yet supported " +
                $"and shouldn't be handled in this converter!")
        };

    private static Result<AttachmentDetails> GetAttachmentDetails(UpdateWrapper wrappedUpdate)
    {
        // These stay proper Exceptions b/c they'd represent totally unexpected behaviour from an external library!
        const string errorMessage = "For Telegram message of type {0} we expect the {0} property to not be null";

        return wrappedUpdate.Message.Type switch
        {
            MessageType.Text or MessageType.Location => new AttachmentDetails(
                Option<string>.None(), Option<AttachmentType>.None()),

            MessageType.Document => new AttachmentDetails(
                wrappedUpdate.Message.Document?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, wrappedUpdate.Message.Type)),
                AttachmentType.Document),

            MessageType.Photo => new AttachmentDetails(
                wrappedUpdate.Message.Photo?.OrderBy(p => p.FileSize).Last().FileId
                ?? throw new InvalidOperationException(
                    string.Format(errorMessage, wrappedUpdate.Message.Type)),
                AttachmentType.Photo),

            MessageType.Voice => new AttachmentDetails(
                wrappedUpdate.Message.Voice?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, wrappedUpdate.Message.Type)),
                AttachmentType.Voice),

            _ => Ui("Attachment type {0} is not yet supported!", wrappedUpdate.Message.Type)
        };
    }

    private record AttachmentDetails(Option<string> FileId, Option<AttachmentType> Type);

    private static Result<Option<Geo>> GetGeoCoordinates(UpdateWrapper wrappedUpdate) =>
        wrappedUpdate.Message.Location switch
        {
            { } location => Option<Geo>.Some(new Geo(
                location.Latitude,
                location.Longitude,
                location.HorizontalAccuracy ?? Option<float>.None())),
            
            _ => Option<Geo>.None() 
        };
    
    private static Result<Option<int>> GetBotCommandEnumCode(UpdateWrapper wrappedUpdate, TlgBotType botType)
    {
        var botCommandEntity = wrappedUpdate.Message.Entities?
            .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

        if (botCommandEntity == null)
            return Option<int>.None();

        if (wrappedUpdate.Message.Text == TlgStart.Command)
            return Option<int>.Some(TlgStart.CommandCode);
        
        var allBotCommandMenus = new BotCommandMenus();

        var botCommandMenuForCurrentBotType = botType switch
        {
            TlgBotType.Operations => allBotCommandMenus.OperationsBotCommandMenu.Values,
            TlgBotType.Communications => allBotCommandMenus.CommunicationsBotCommandMenu.Values,
            TlgBotType.Notifications => allBotCommandMenus.NotificationsBotCommandMenu.Values,
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };

        var botCommandFromTelegramUpdate = botCommandMenuForCurrentBotType
            .SelectMany(kvp => kvp.Values)
            .FirstOrDefault(mbc => mbc.Command == wrappedUpdate.Message.Text);
        
        if (botCommandFromTelegramUpdate == null)
            return UiConcatenate(
                Ui("The BotCommand {0} does not exist for the {1}Bot [errcode: {2}]. ", 
                    wrappedUpdate.Message.Text ?? "[empty text!]", botType, "W3DL9"),
                IUpdateProcessor.SeeValidBotCommandsInstruction);

        var botCommandUnderlyingEnumCodeForBotTypeAgnosticRepresentation = botType switch
        {
            TlgBotType.Operations => Option<int>.Some(
                (int) allBotCommandMenus.OperationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromTelegramUpdate))
                    .Key),
            TlgBotType.Communications => Option<int>.Some(
                (int) allBotCommandMenus.CommunicationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromTelegramUpdate))
                    .Key),
            TlgBotType.Notifications => Option<int>.Some(
                (int) allBotCommandMenus.NotificationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromTelegramUpdate))
                    .Key),
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };

        return botCommandUnderlyingEnumCodeForBotTypeAgnosticRepresentation;
    }

    private static Result<Option<int>> GetDomainCategoryEnumCode(UpdateWrapper wrappedUpdate)
    {
        return int.TryParse(wrappedUpdate.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData <= EnumCallbackId.DomainCategoryMaxThreshold
                ? callBackData
                : Option<int>.None()
            : Option<int>.None();
    }
    
    private static Result<Option<long>> GetControlPromptEnumCode(UpdateWrapper wrappedUpdate)
    {
        return long.TryParse(wrappedUpdate.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData > EnumCallbackId.DomainCategoryMaxThreshold
                ? callBackData
                : Option<long>.None()
            : Option<long>.None();
    }
    
    private async Task<Result<TlgUpdate>> GetTelegramUpdateAsync(
        UpdateWrapper wrappedUpdate,
        TlgBotType botType,
        TlgUpdateType tlgUpdateType,
        AttachmentDetails attachmentDetails,
        Option<Geo> geoCoordinates,
        Option<int> botCommandEnumCode,
        Option<int> domainCategoryEnumCode,
        Option<long> controlPromptEnumCode)
    {
        if (wrappedUpdate.Message.From?.Id == null || 
            string.IsNullOrWhiteSpace(wrappedUpdate.Message.Text) 
            && attachmentDetails.FileId.IsNone
            && tlgUpdateType != TlgUpdateType.Location)
        {
            return Ui("A valid message must a) have a User Id ('From.Id' in Telegram); " +
                                         "b) either have a text or an attachment (unless it's a Location).");   
        }
        
        TlgUserId userId = wrappedUpdate.Message.From.Id;
        TlgChatId chatId = wrappedUpdate.Message.Chat.Id;

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
        
        var messageText = !string.IsNullOrWhiteSpace(wrappedUpdate.Message.Text)
            ? wrappedUpdate.Message.Text
            : wrappedUpdate.Message.Caption;
        
        return new TlgUpdate(userId, chatId, botType, tlgUpdateType,
            new TlgUpdateDetails(
                wrappedUpdate.Message.Date,
                wrappedUpdate.Message.MessageId,
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
