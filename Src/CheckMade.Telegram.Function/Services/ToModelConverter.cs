using CheckMade.Telegram.Logic.RequestProcessors;
using CheckMade.Telegram.Model;
using CheckMade.Telegram.Model.BotCommands;
using CheckMade.Telegram.Model.DTOs;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace CheckMade.Telegram.Function.Services;

public interface IToModelConverter
{
    Task<Attempt<InputMessageDto>> SafelyConvertMessageAsync(Message telegramInputMessage, BotType botType);
}

internal class ToModelConverter(ITelegramFilePathResolver filePathResolver) : IToModelConverter
{
    public async Task<Attempt<InputMessageDto>> SafelyConvertMessageAsync(Message telegramInputMessage, BotType botType)
    {
        return (await
                    (from attachmentDetails
                            in SafelyGetAttachmentDetails(telegramInputMessage)
                        from botCommandEnumCode
                            in GetBotCommandEnumCode(telegramInputMessage, botType)
                        from modelInputMessage
                            in GetInputMessageAsync(telegramInputMessage, botType, attachmentDetails,
                                botCommandEnumCode)
                        select modelInputMessage))
            .Match(
            modelInputMessage => modelInputMessage,
            failure => Attempt<InputMessageDto>.Fail(
                failure with // preserves any contained Exception and prefixes any contained Error UiString
                {
                    Error = UiConcatenate(
                        Ui("Failed to convert Telegram Message to Model. "),
                        failure.Error)
                }
            ));
    } 

    private static Attempt<AttachmentDetails> SafelyGetAttachmentDetails(Message telegramInputMessage)
    {
        // These stay proper Exceptions b/c they'd represent totally unexpected behaviour from an external library!
        const string errorMessage = "For Telegram message of type {0} we expect the {0} property to not be null";

        return telegramInputMessage.Type switch
        {
            MessageType.Text => new AttachmentDetails(
                Option<string>.None(), Option<AttachmentType>.None()),
            
            MessageType.Audio => Attempt<AttachmentDetails>.Run(() => new AttachmentDetails(
                telegramInputMessage.Audio?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, telegramInputMessage.Type)), 
                AttachmentType.Audio)),
            
            MessageType.Photo => Attempt<AttachmentDetails>.Run(() => new AttachmentDetails(
                telegramInputMessage.Photo?.OrderBy(p => p.FileSize).Last().FileId 
                ?? throw new InvalidOperationException(
                    string.Format(errorMessage, telegramInputMessage.Type)), 
                    AttachmentType.Photo)),
            
            MessageType.Document => Attempt<AttachmentDetails>.Run(() => new AttachmentDetails(
                telegramInputMessage.Document?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, telegramInputMessage.Type)), 
                    AttachmentType.Document)),
            
            MessageType.Video => Attempt<AttachmentDetails>.Run(() => new AttachmentDetails(
                telegramInputMessage.Video?.FileId ?? throw new InvalidOperationException(
                    string.Format(errorMessage, telegramInputMessage.Type)),
                AttachmentType.Video)),
            
            _ => new Failure(Error:
                Ui("Attachment type {0} is not yet supported!", telegramInputMessage.Type)) 
        };
    }

    private record AttachmentDetails(Option<string> FileId, Option<AttachmentType> Type);

    private Attempt<Option<int>> GetBotCommandEnumCode(
        Message telegramInputMessage,
        BotType botType)
    {
        var botCommandEntity = telegramInputMessage.Entities?
            .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand);

        if (botCommandEntity == null)
            return Option<int>.None();

        if (telegramInputMessage.Text == Start.Command)
            return Option<int>.Some(Start.CommandCode);
        
        var allBotCommandMenus = new BotCommandMenus();

        var botCommandMenuForCurrentBotType = botType switch
        {
            BotType.Submissions => allBotCommandMenus.SubmissionsBotCommandMenu.Values,
            BotType.Communications => allBotCommandMenus.CommunicationsBotCommandMenu.Values,
            BotType.Notifications => allBotCommandMenus.NotificationsBotCommandMenu.Values,
            _ => throw new ArgumentOutOfRangeException(nameof(botType))
        };

        var botCommandFromInputMessage = botCommandMenuForCurrentBotType
            .SelectMany(kvp => kvp.Values)
            .FirstOrDefault(mbc => mbc.Command == telegramInputMessage.Text);
        
        if (botCommandFromInputMessage == null)
            return new Failure (Error: UiConcatenate(
                Ui("The BotCommand {0} does not exist for the {1}Bot [errcode: {2}]. ", 
                    telegramInputMessage.Text ?? "[empty text!]", botType, "W3DL9"),
                IRequestProcessor.SeeValidBotCommandsInstruction));

        var botCommandUnderlyingEnumCodeForBotTypeAgnosticRepresentation = botType switch
        {
            BotType.Submissions => Option<int>.Some(
                (int) allBotCommandMenus.SubmissionsBotCommandMenu
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
    
    private async Task<Attempt<InputMessageDto>> GetInputMessageAsync(
        Message telegramInputMessage,
        BotType botType,
        AttachmentDetails attachmentDetails,
        Option<int> botCommandEnumCode)
    {
        var userId = telegramInputMessage.From?.Id;

        if (userId == null || string.IsNullOrWhiteSpace(telegramInputMessage.Text) && attachmentDetails.FileId.IsNone)
        {
            return new Failure(Error: Ui("A valid message must a) have a User Id ('From.Id' in Telegram); " +
                                         "b) either have a text or an attachment."));   
        }

        var telegramAttachmentUrl = Option<string>.None();
        
        if (attachmentDetails.FileId.IsSome)
        {
            var pathAttempt = await filePathResolver.SafelyGetTelegramFilePathAsync(
                attachmentDetails.FileId.GetValueOrDefault());
            
            if (pathAttempt.IsFailure)
                return new Failure(Error:
                    Ui("Error while trying to retrieve full Telegram server path to attachment file."));

            telegramAttachmentUrl = pathAttempt.GetValueOrDefault();
        }
        
        var messageText = !string.IsNullOrWhiteSpace(telegramInputMessage.Text)
            ? telegramInputMessage.Text
            : telegramInputMessage.Caption;
        
        return new InputMessageDto(userId.Value, telegramInputMessage.Chat.Id, botType, 
            new InputMessageDetails(
                telegramInputMessage.Date, 
                !string.IsNullOrWhiteSpace(messageText) ? messageText : Option<string>.None(), 
                telegramAttachmentUrl,
                attachmentDetails.Type,
                botCommandEnumCode));
    }
}
