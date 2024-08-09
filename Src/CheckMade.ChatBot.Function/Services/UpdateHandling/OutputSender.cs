using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Utils.UiTranslation;
using Telegram.Bot.Types;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

internal static class OutputSender
{
        internal static async Task<Unit> SendOutputsAsync(
            IReadOnlyCollection<OutputDto> outputs,
            IDictionary<InteractionMode, IBotClientWrapper> botClientByMode,
            InteractionMode currentInteractionMode,
            ChatId currentChatId,
            IReadOnlyCollection<TlgAgentRoleBind> activeRoleBindings,
            IUiTranslator uiTranslator,
            IOutputToReplyMarkupConverter converter,
            IBlobLoader blobLoader)
    {
        Func<IReadOnlyCollection<OutputDto>, Task> sendOutputsInSeriesAndOriginalOrder 
            = async outputsPerPort =>
            {
                foreach (var output in outputsPerPort)
                {
                    /*
                     * LogicalPort will typically not be set explicitly in these cases:
                     * a) For an update from a User who hasn't done the UserAuth workflow yet i.e. is unknown
                     * b) For outputs aimed at the originator of the last input i.e. the default case
                     * => LogicalPorts therefore mostly only used to send e.g. notifications to other users
                     */
                
                    var outputMode = output.LogicalPort.Match(
                        logicalPort => logicalPort.InteractionMode,
                        () => currentInteractionMode);

                    var outputBotClient = botClientByMode[outputMode];

                    /* .First() instead of .FirstOrDefault() b/c I want it to crash 'fast & hard' if my assumption is
                     broken that the business logic only sets a LogicalPort for a role & mode that is, at the time,
                     mapped to a TlgAgent  */
                    var outputChatId = output.LogicalPort.Match(
                        logicalPort => activeRoleBindings
                            .First(tarb => 
                                tarb.Role.Equals(logicalPort.Role) &&
                                tarb.TlgAgent.Mode == outputMode)
                            .TlgAgent.ChatId.Id,
                        () => currentChatId);

                    switch (output)
                    {
                        case
                        {
                            Text.IsSome: true, Attachments.IsSome: false, 
                            Location.IsSome: false, UpdateExistingOutputMessageId.IsSome: false
                        }: 
                            await InvokeSendTextMessageAsync(output.Text.GetValueOrThrow());
                            break;

                        case { UpdateExistingOutputMessageId.IsSome: true }:
                            await InvokeEditTextMessageAsync(output);
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
                        await outputBotClient
                            .SendTextMessageAsync(
                                outputChatId,
                                uiTranslator.Translate(Ui("Please choose:")),
                                uiTranslator.Translate(outputText),
                                converter.GetReplyMarkup(output));
                    }

                    async Task InvokeEditTextMessageAsync(OutputDto outputWithUpdatedMessage)
                    {
                        await outputBotClient
                            .EditTextMessageAsync(
                                outputChatId,
                                outputWithUpdatedMessage.Text.IsSome
                                    ? uiTranslator.Translate(outputWithUpdatedMessage.Text.GetValueOrThrow())
                                    : Option<string>.None(),
                                outputWithUpdatedMessage.UpdateExistingOutputMessageId.GetValueOrThrow(),
                                converter.GetReplyMarkup(outputWithUpdatedMessage));
                    }
                
                    async Task InvokeSendAttachmentAsync(AttachmentDetails details)
                    {
                        var (blobData, fileName) =
                            await blobLoader.DownloadBlobAsync(details.AttachmentUri);
                        var fileStream = new InputFileStream(blobData, fileName);
                    
                        var caption = details.Caption.Match(
                            value => value,
                            Option<string>.None);

                        var attachmentSendOutParams = new AttachmentSendOutParameters(
                            outputChatId,
                            fileStream,
                            caption,
                            converter.GetReplyMarkup(output)
                        );

                        switch (details.AttachmentType)
                        {
                            case TlgAttachmentType.Document:
                                await outputBotClient.SendDocumentAsync(attachmentSendOutParams);
                                break;

                            case TlgAttachmentType.Photo:
                                await outputBotClient.SendPhotoAsync(attachmentSendOutParams);
                                break;

                            case TlgAttachmentType.Voice:
                                await outputBotClient.SendVoiceAsync(attachmentSendOutParams);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException(nameof(details.AttachmentType));
                        }
                    }

                    async Task InvokeSendLocationAsync(Geo location)
                    {
                        await outputBotClient
                            .SendLocationAsync(
                                outputChatId,
                                location,
                                converter.GetReplyMarkup(output));
                    }
                }
            };

        var outputGroups = outputs.GroupBy(o => o.LogicalPort);

        var parallelTasks = outputGroups
            .Select(outputsPerLogicalPortGroup => 
                sendOutputsInSeriesAndOriginalOrder
                    .Invoke(outputsPerLogicalPortGroup
                        .ToImmutableReadOnlyCollection()));
        
        /* 1) Waits for all parallel executing tasks (generated by .Select()), to complete
         * 2) The 'await' unwraps the resulting aggregate Task object and rethrows any Exceptions */
        await Task.WhenAll(parallelTasks);
        
        return Unit.Value;
    }
}