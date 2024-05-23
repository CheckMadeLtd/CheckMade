using CheckMade.Common.LangExt.MonadicWrappers;
using CheckMade.Common.Utils;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IToModelConverter
{
    Task<InputMessage> ConvertMessageOrThrowAsync(Message telegramInputMessage, BotType botType);
}

internal class ToModelConverter(ITelegramFilePathResolver filePathResolver) : IToModelConverter
{
    public async Task<InputMessage> ConvertMessageOrThrowAsync(Message telegramInputMessage, BotType botType)
    {
        return ((Result<InputMessage>) await
            (from attachmentDetails 
                in GetAttachmentDetails(telegramInputMessage)
            from botCommandEnumCode 
                in GetBotCommandEnumCode(telegramInputMessage, botType)
            from modelInputMessage 
                in GetInputMessageAsync(telegramInputMessage, botType, attachmentDetails, botCommandEnumCode)
            select modelInputMessage))
            .Match(
                modelInputMessage => modelInputMessage,
                error => throw new ToModelConversionException(
                    $"Failed to convert Telegram Message to Model: {error}"));
    } 

    // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
    private static Result<AttachmentDetails> GetAttachmentDetails(Message telegramInputMessage)
    {
        // These stay proper Exceptions b/c they'd represent totally unexpected behaviour from an external library!
        const string errorMessage = "For Telegram message of type {0} we expect the {0} property to not be null";

        return telegramInputMessage.Type switch
        {
            MessageType.Text => Result<AttachmentDetails>.FromSuccess(
                new AttachmentDetails(Option<string>.None(), Option<AttachmentType>.None())),
            
            MessageType.Audio => Result<AttachmentDetails>.FromSuccess(
                new AttachmentDetails(telegramInputMessage.Audio?.FileId 
                                      ?? throw new InvalidOperationException(
                                          string.Format(errorMessage, telegramInputMessage.Type)), 
                    AttachmentType.Audio)),
            
            MessageType.Photo => Result<AttachmentDetails>.FromSuccess(
                new AttachmentDetails(telegramInputMessage.Photo?.OrderBy(p => p.FileSize).Last().FileId
                                       ?? throw new InvalidOperationException(
                                           string.Format(errorMessage, telegramInputMessage.Type)), 
                    AttachmentType.Photo)),
            
            MessageType.Document => Result<AttachmentDetails>.FromSuccess(
                new AttachmentDetails(telegramInputMessage.Document?.FileId
                                       ?? throw new InvalidOperationException(
                                           string.Format(errorMessage, telegramInputMessage.Type)), 
                    AttachmentType.Document)),
            
            MessageType.Video => Result<AttachmentDetails>.FromSuccess(
                new AttachmentDetails(telegramInputMessage.Video?.FileId
                                       ?? throw new InvalidOperationException(
                                           string.Format(errorMessage, telegramInputMessage.Type)),
                AttachmentType.Video)),
            
            _ => Result<AttachmentDetails>.FromError(
                $"Attachment type {telegramInputMessage.Type} is not yet supported!") 
        };
    }

    private record AttachmentDetails(Option<string> FileId, Option<AttachmentType> Type);

    internal static readonly string BotCommandDoesNotExistError = 
        UiSm("Der BotCommand {0} existiert nicht für den {1} bot.");
    
    private static Result<Option<int>> GetBotCommandEnumCode(
        Message telegramInputMessage,
        BotType botType)
    {
        var botCommandEntity = telegramInputMessage.Entities?
            .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

        if (botCommandEntity == null)
            return Result<Option<int>>.FromSuccess(Option<int>.None());

        if (telegramInputMessage.Text == Start.Command)
        {
            return Result<Option<int>>.FromSuccess(Option<int>.Some(1));
        }
        
        var allBotCommandMenus = new BotCommandMenus();

        var botCommandMenuForCurrentBotType = botType switch
        {
            BotType.Submissions => allBotCommandMenus.SubmissionsBotCommandMenu.Values,
            BotType.Communications => allBotCommandMenus.CommunicationsBotCommandMenu.Values,
            BotType.Notifications => allBotCommandMenus.NotificationsBotCommandMenu.Values,
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };

        var botCommandFromInputMessage = botCommandMenuForCurrentBotType
            .FirstOrDefault(mbc => mbc.Command == telegramInputMessage.Text)?.Command;
        
        if (botCommandFromInputMessage == null)
            return Result<Option<int>>.FromError(
                string.Format(BotCommandDoesNotExistError, telegramInputMessage.Text, botType));

        var botCommandUnderlyingEnumCodeForBotTypeAgnosticRepresentation = botType switch
        {
            BotType.Submissions => Result<Option<int>>.FromSuccess(
                (int) allBotCommandMenus.SubmissionsBotCommandMenu
                .First(kvp => 
                    kvp.Value.Command == botCommandFromInputMessage)
                .Key),
            BotType.Communications => Result<Option<int>>.FromSuccess(
                (int) allBotCommandMenus.CommunicationsBotCommandMenu
                .First(kvp => 
                    kvp.Value.Command == botCommandFromInputMessage)
                .Key),
            BotType.Notifications => Result<Option<int>>.FromSuccess(
                (int) allBotCommandMenus.NotificationsBotCommandMenu
                .First(kvp => 
                    kvp.Value.Command == botCommandFromInputMessage)
                .Key),
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };

        return botCommandUnderlyingEnumCodeForBotTypeAgnosticRepresentation;
    }
    
    private async Task<Result<InputMessage>> GetInputMessageAsync(
        Message telegramInputMessage,
        BotType botType,
        AttachmentDetails attachmentDetails,
        Option<int> botCommandEnumCode)
    {
        var userId = telegramInputMessage.From?.Id; 
                     
        if (userId == null)
            return Result<InputMessage>.FromError("User Id (From.Id in the input message) must not be null");

        if (string.IsNullOrWhiteSpace(telegramInputMessage.Text) && attachmentDetails.FileId.IsNone)
        {
            return Result<InputMessage>.FromError(
                "A valid message must either have a text or an attachment - both must not be null/empty");
        }

        var telegramAttachmentUrl = Option<string>.None();
        
        if (attachmentDetails.FileId.IsSome)
        {
            var pathAttempt = await filePathResolver.SafelyGetTelegramFilePathAsync(
                attachmentDetails.FileId.GetValueOrDefault());
            
            if (pathAttempt.IsFailure)
                return Result<InputMessage>.FromError(
                    "Error while trying to retrieve full Telegram server path to attachment file.");

            telegramAttachmentUrl = pathAttempt.GetValueOrDefault();
        }
        
        var messageText = !string.IsNullOrWhiteSpace(telegramInputMessage.Text)
            ? telegramInputMessage.Text
            : telegramInputMessage.Caption;
        
        return Result<InputMessage>.FromSuccess(
            new InputMessage(userId.Value,
                telegramInputMessage.Chat.Id,
                new MessageDetails(
                    telegramInputMessage.Date,
                    botType,
                    !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(),
                    telegramAttachmentUrl,
                    attachmentDetails.Type,
                    botCommandEnumCode)));
    }
}
