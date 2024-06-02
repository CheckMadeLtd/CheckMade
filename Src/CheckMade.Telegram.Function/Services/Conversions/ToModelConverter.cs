using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Telegram.Function.Services.UpdateHandling;
using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model.BotCommand;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services.Conversions;

public interface IToModelConverter
{
    Task<Attempt<InputMessageDto>> ConvertToModelAsync(UpdateWrapper telegramUpdate, BotType botType);
}

internal class ToModelConverter(ITelegramFilePathResolver filePathResolver) : IToModelConverter
{
    public async Task<Attempt<InputMessageDto>> ConvertToModelAsync(UpdateWrapper telegramUpdate, BotType botType)
    {
        return (await
                (from modelUpdateType
                        in GetModelUpdateType(telegramUpdate)
                    from attachmentDetails
                        in GetAttachmentDetails(telegramUpdate)
                    from geoCoordinates 
                        in GetGeoCoordinates(telegramUpdate)
                    from botCommandEnumCode
                        in GetBotCommandEnumCode(telegramUpdate, botType)
                    from domainCategoryEnumCode
                        in GetDomainCategoryEnumCode(telegramUpdate)
                    from controlPromptEnumCode
                        in GetControlPromptEnumCode(telegramUpdate)
                    from modelInputMessage
                        in GetInputMessageAsync(
                            telegramUpdate,
                            botType,
                            modelUpdateType,
                            attachmentDetails,
                            geoCoordinates,
                            botCommandEnumCode,
                            domainCategoryEnumCode,
                            controlPromptEnumCode)
                    select modelInputMessage))
            .Match(
                modelInputMessage => modelInputMessage,
                error => Attempt<InputMessageDto>.Fail(
                    error with // preserves any contained Exception and prefixes any contained Error UiString
                    {
                        FailureMessage = UiConcatenate(
                            Ui("Failed to convert Telegram Message to Model. "),
                            error.FailureMessage)
                    }
                ));
    }

    private static Attempt<ModelUpdateType> GetModelUpdateType(UpdateWrapper telegramUpdate) =>
        telegramUpdate.Update.Type switch
        {
            UpdateType.Message or UpdateType.EditedMessage => telegramUpdate.Message.Type switch
            {
                MessageType.Text => telegramUpdate.Message.Entities?[0].Type switch
                {
                    MessageEntityType.BotCommand => ModelUpdateType.CommandMessage,
                    _ => ModelUpdateType.TextMessage
                },
                MessageType.Location => ModelUpdateType.Location,
                _ => ModelUpdateType.AttachmentMessage
            },

            UpdateType.CallbackQuery => ModelUpdateType.CallbackQuery,

            _ => throw new InvalidOperationException(
                $"Telegram Update of type {telegramUpdate.Update.Type} is not yet supported " +
                $"and shouldn't be handled in this converter!")
        };

    private static Attempt<AttachmentDetails> GetAttachmentDetails(UpdateWrapper telegramUpdate)
    {
        // These stay proper Exceptions b/c they'd represent totally unexpected behaviour from an external library!
        const string errorMessage = "For Telegram message of type {0} we expect the {0} property to not be null";

        return telegramUpdate.Message.Type switch
        {
            MessageType.Text or MessageType.Location => new AttachmentDetails(
                Option<string>.None(), Option<AttachmentType>.None()),

            MessageType.Audio => Attempt<AttachmentDetails>.Run(() => new AttachmentDetails(
                telegramUpdate.Message.Audio?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, telegramUpdate.Message.Type)),
                AttachmentType.Audio)),

            MessageType.Document => Attempt<AttachmentDetails>.Run(() => new AttachmentDetails(
                telegramUpdate.Message.Document?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, telegramUpdate.Message.Type)),
                AttachmentType.Document)),

            MessageType.Photo => Attempt<AttachmentDetails>.Run(() => new AttachmentDetails(
                telegramUpdate.Message.Photo?.OrderBy(p => p.FileSize).Last().FileId
                ?? throw new InvalidOperationException(
                    string.Format(errorMessage, telegramUpdate.Message.Type)),
                AttachmentType.Photo)),

            _ => new Error(FailureMessage:
                Ui("Attachment type {0} is not yet supported!", telegramUpdate.Message.Type))
        };
    }

    private record AttachmentDetails(Option<string> FileId, Option<AttachmentType> Type);

    private static Attempt<Option<Geo>> GetGeoCoordinates(UpdateWrapper telegramUpdate) =>
        telegramUpdate.Message.Location switch
        {
            { } location => Option<Geo>.Some(new Geo(
                location.Latitude,
                location.Longitude,
                location.HorizontalAccuracy ?? Option<float>.None())),
            
            _ => Option<Geo>.None() 
        };
    
    private static Attempt<Option<int>> GetBotCommandEnumCode(
        UpdateWrapper telegramUpdate,
        BotType botType)
    {
        var botCommandEntity = telegramUpdate.Message.Entities?
            .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

        if (botCommandEntity == null)
            return Option<int>.None();

        if (telegramUpdate.Message.Text == Start.Command)
            return Option<int>.Some(Start.CommandCode);
        
        var allBotCommandMenus = new BotCommandMenus();

        var botCommandMenuForCurrentBotType = botType switch
        {
            BotType.Operations => allBotCommandMenus.OperationsBotCommandMenu.Values,
            BotType.Communications => allBotCommandMenus.CommunicationsBotCommandMenu.Values,
            BotType.Notifications => allBotCommandMenus.NotificationsBotCommandMenu.Values,
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };

        var botCommandFromInputMessage = botCommandMenuForCurrentBotType
            .SelectMany(kvp => kvp.Values)
            .FirstOrDefault(mbc => mbc.Command == telegramUpdate.Message.Text);
        
        if (botCommandFromInputMessage == null)
            return new Error (FailureMessage: UiConcatenate(
                Ui("The BotCommand {0} does not exist for the {1}Bot [errcode: {2}]. ", 
                    telegramUpdate.Message.Text ?? "[empty text!]", botType, "W3DL9"),
                IRequestProcessor.SeeValidBotCommandsInstruction));

        var botCommandUnderlyingEnumCodeForBotTypeAgnosticRepresentation = botType switch
        {
            BotType.Operations => Option<int>.Some(
                (int) allBotCommandMenus.OperationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromInputMessage))
                    .Key),
            BotType.Communications => Option<int>.Some(
                (int) allBotCommandMenus.CommunicationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromInputMessage))
                    .Key),
            BotType.Notifications => Option<int>.Some(
                (int) allBotCommandMenus.NotificationsBotCommandMenu
                    .First(kvp => 
                        kvp.Value.Values.Contains(botCommandFromInputMessage))
                    .Key),
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };

        return botCommandUnderlyingEnumCodeForBotTypeAgnosticRepresentation;
    }

    private static Attempt<Option<int>> GetDomainCategoryEnumCode(UpdateWrapper telegramUpdate)
    {
        return int.TryParse(telegramUpdate.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData <= EnumCallbackId.DomainCategoryMaxThreshold
                ? Attempt<Option<int>>.Succeed(callBackData)
                : Attempt<Option<int>>.Succeed(Option<int>.None())
            : Attempt<Option<int>>.Succeed(Option<int>.None());
    }
    
    private static Attempt<Option<long>> GetControlPromptEnumCode(UpdateWrapper telegramUpdate)
    {
        return long.TryParse(telegramUpdate.Update.CallbackQuery?.Data, out var callBackData)
            ? callBackData > EnumCallbackId.DomainCategoryMaxThreshold
                ? Attempt<Option<long>>.Succeed(callBackData)
                : Attempt<Option<long>>.Succeed(Option<long>.None())
            : Attempt<Option<long>>.Succeed(Option<long>.None());
    }
    
    private async Task<Attempt<InputMessageDto>> GetInputMessageAsync(
        UpdateWrapper telegramUpdate,
        BotType botType,
        ModelUpdateType modelUpdateType,
        AttachmentDetails attachmentDetails,
        Option<Geo> geoCoordinates,
        Option<int> botCommandEnumCode,
        Option<int> domainCategoryEnumCode,
        Option<long> controlPromptEnumCode)
    {
        if (telegramUpdate.Message.From?.Id == null || 
            string.IsNullOrWhiteSpace(telegramUpdate.Message.Text) 
            && attachmentDetails.FileId.IsNone
            && modelUpdateType != ModelUpdateType.Location)
        {
            return new Error(FailureMessage: Ui("A valid message must a) have a User Id ('From.Id' in Telegram); " +
                                         "b) either have a text or an attachment (unless it's a Location)."));   
        }
        
        TelegramUserId userId = telegramUpdate.Message.From.Id;
        TelegramChatId chatId = telegramUpdate.Message.Chat.Id;

        var telegramAttachmentUrl = Option<string>.None();
        
        if (attachmentDetails.FileId.IsSome)
        {
            var pathAttempt = await filePathResolver.GetTelegramFilePathAsync(
                attachmentDetails.FileId.GetValueOrDefault());
            
            if (pathAttempt.IsError)
                return new Error(FailureMessage:
                    Ui("Error while trying to retrieve full Telegram server path to attachment file."));

            telegramAttachmentUrl = pathAttempt.GetValueOrDefault();
        }
        
        var messageText = !string.IsNullOrWhiteSpace(telegramUpdate.Message.Text)
            ? telegramUpdate.Message.Text
            : telegramUpdate.Message.Caption;
        
        return new InputMessageDto(userId, chatId, botType, modelUpdateType,
            new InputMessageDetails(
                telegramUpdate.Message.Date,
                telegramUpdate.Message.MessageId,
                !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(), 
                telegramAttachmentUrl,
                attachmentDetails.Type,
                geoCoordinates,
                botCommandEnumCode,
                domainCategoryEnumCode,
                controlPromptEnumCode));
    }
}
