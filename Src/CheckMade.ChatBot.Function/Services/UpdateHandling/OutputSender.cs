using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;
using CheckMade.Common.Utils.UiTranslation;
using Telegram.Bot.Types;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

internal static class OutputSender
{
        internal static async Task<Unit> SendOutputsAsync(
            IReadOnlyList<OutputDto> outputs,
            IDictionary<InteractionMode, IBotClientWrapper> botClientByMode,
            InteractionMode currentlyReceivingInteractionMode,
            ChatId currentlyReceivingChatId,
            IEnumerable<TlgClientPortModeRole> tlgClientPortModeRole,
            IUiTranslator uiTranslator,
            IOutputToReplyMarkupConverter converter,
            IBlobLoader blobLoader)
    {
        Func<IReadOnlyCollection<OutputDto>, Task> sendOutputsInSeriesAndOriginalOrder 
            = async outputsPerPort =>
        {
            foreach (var output in outputsPerPort)
            {
                var relevantMode = output.LogicalPort.Match(
                    logicalPort => logicalPort.InteractionMode,
                    // e.g. for a virgin, pre-auth update
                    () => currentlyReceivingInteractionMode);

                var portBotClient = botClientByMode[relevantMode];

                var portChatId = output.LogicalPort.Match(
                    logicalPort => tlgClientPortModeRole
                        .First(cpmr => 
                            cpmr.Role == logicalPort.Role &&
                            cpmr.Mode == relevantMode &&
                            cpmr.Status == DbRecordStatus.Active)
                        .ClientPort.ChatId.Id,
                    // e.g. for a virgin, pre-auth update
                    () => currentlyReceivingChatId);
                    
                switch (output)
                {
                    case { Text.IsSome: true, Attachments.IsSome: false, Location.IsSome: false }:
                        await InvokeSendTextMessageAsync(output.Text.GetValueOrThrow());
                        break;

                    case { Attachments.IsSome: true }:
                        if (output.Text.IsSome)
                            await InvokeSendTextMessageAsync(output.Text.GetValueOrThrow());
                        foreach (var attachment in output.Attachments.GetValueOrThrow())
                            await InvokeSendAttachmentAsync(attachment);
                        break;

                    case { Location.IsSome: true }:
                        await InvokeSendLocationAsync(output.Location.GetValueOrThrow());
                        break;
                }

                continue;

                async Task InvokeSendTextMessageAsync(UiString outputText)
                {
                    await portBotClient
                        .SendTextMessageAsync(
                            portChatId,
                            uiTranslator.Translate(Ui("Please choose:")),
                            uiTranslator.Translate(outputText),
                            converter.GetReplyMarkup(output));
                }

                async Task InvokeSendAttachmentAsync(OutputAttachmentDetails details)
                {
                    var (blobData, fileName) =
                        await blobLoader.DownloadBlobAsync(details.AttachmentUri);
                    var fileStream = new InputFileStream(blobData, fileName);
                    
                    var caption = details.Caption.Match(
                        value => Option<string>.Some(uiTranslator.Translate(value)),
                        Option<string>.None);

                    var attachmentSendOutParams = new AttachmentSendOutParameters(
                        portChatId,
                        fileStream,
                        caption,
                        converter.GetReplyMarkup(output)
                    );

                    switch (details.AttachmentType)
                    {
                        case TlgAttachmentType.Document:
                            await portBotClient.SendDocumentAsync(attachmentSendOutParams);
                            break;

                        case TlgAttachmentType.Photo:
                            await portBotClient.SendPhotoAsync(attachmentSendOutParams);
                            break;

                        case TlgAttachmentType.Voice:
                            await portBotClient.SendVoiceAsync(attachmentSendOutParams);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(details.AttachmentType));
                    }
                }

                async Task InvokeSendLocationAsync(Geo location)
                {
                    await portBotClient
                        .SendLocationAsync(
                            portChatId,
                            location,
                            converter.GetReplyMarkup(output));
                }
            }
        };

        var outputGroups = outputs.GroupBy(o => o.LogicalPort);

        var parallelTasks = outputGroups
            .Select(outputsPerLogicalPortGroup => 
                sendOutputsInSeriesAndOriginalOrder.Invoke(outputsPerLogicalPortGroup.ToList().AsReadOnly()));
        
        /* 1) Waits for all parallel executing tasks (generated by .Select()), to complete
         * 2) The 'await' unwraps the resulting aggregate Task object and rethrows any Exceptions */
        await Task.WhenAll(parallelTasks);
        
        return Unit.Value;
    }
}