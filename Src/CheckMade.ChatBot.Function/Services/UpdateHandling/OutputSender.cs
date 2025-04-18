using System.Collections.Immutable;
using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Utils.UiTranslation;
using Telegram.Bot.Types;

namespace CheckMade.ChatBot.Function.Services.UpdateHandling;

internal static class OutputSender
{
    internal static async Task<IReadOnlyCollection<OutputDto>> SendOutputsAsync(
        IReadOnlyCollection<OutputDto> outputs,
        IDictionary<InteractionMode, IBotClientWrapper> botClientByMode,
        InteractionMode currentInteractionMode,
        ChatId currentChatId,
        IReadOnlyCollection<TlgAgentRoleBind> activeRoleBindings,
        IUiTranslator uiTranslator,
        IOutputToReplyMarkupConverter converter,
        IBlobLoader blobLoader)
    {
        Func<IReadOnlyCollection<OutputDto>, Task<IReadOnlyCollection<OutputDto>>> sendOutputsInSeriesAndOriginalOrder 
            = async outputsPerPort =>
            {
                List<OutputDto> sentOutputs = [];
                
                foreach (var output in outputsPerPort)
                {
                    // LogicalPort is only set for outputs NOT aimed at the originator of the current input
                    var outputMode = output.LogicalPort.Match(
                        static logicalPort => logicalPort.InteractionMode,
                        () => currentInteractionMode);

                    var outputBotClient = botClientByMode[outputMode];
                    
                    // Crashes hard when assumption broken, that LogicalPort only set for roles with TlgAgentRoleBind
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
                            sentOutputs.Add(
                                GetOutputEnrichedWithActualSendOutParams(
                                    await InvokeSendTextMessageAsync()));
                            break;

                        // For Outputs with the sole purpose of updating a previous message (e.g., removing buttons)
                        // By definition, they wouldn't need to have Attachments, Locations, etc.
                        case { UpdateExistingOutputMessageId.IsSome: true }:
                            await InvokeEditTextMessageAsync();
                            sentOutputs.Add(
                                GetOutputEnrichedWithActualSendOutParams(
                                    output.UpdateExistingOutputMessageId.GetValueOrThrow()));
                            break;
                    
                        case { Attachments.IsSome: true }:

                            TlgMessageId? messageId = null;
                            
                            if (output.Text.IsSome)
                                messageId = await InvokeSendTextMessageAsync();
                            
                            foreach (var attachment in output.Attachments.GetValueOrThrow())
                                messageId = await InvokeSendAttachmentAsync(attachment);
                            
                            sentOutputs.Add(
                                GetOutputEnrichedWithActualSendOutParams(messageId!));
                            break;

                        case { Location.IsSome: true }:
                            sentOutputs.Add(
                                GetOutputEnrichedWithActualSendOutParams(
                                    await InvokeSendLocationAsync()));
                            break;
                    }

                    continue;

                    OutputDto GetOutputEnrichedWithActualSendOutParams(TlgMessageId messageId) =>
                        output with
                        {
                            ActualSendOutParams = new ActualSendOutParams
                            {
                                TlgMessageId = messageId,
                                ChatId = outputChatId.Identifier!
                            }
                        };
                    
                    async Task<TlgMessageId> InvokeSendTextMessageAsync()
                    {
                        return await outputBotClient
                            .SendTextMessageAsync(
                                outputChatId,
                                uiTranslator.Translate(Ui("Please choose:")),
                                uiTranslator.Translate(output.Text.GetValueOrThrow()),
                                converter.GetReplyMarkup(output));
                    }

                    async Task InvokeEditTextMessageAsync()
                    {
                        await outputBotClient
                            .EditTextMessageAsync(
                                outputChatId,
                                output.Text.IsSome
                                    ? uiTranslator.Translate(output.Text.GetValueOrThrow())
                                    : Option<string>.None(),
                                output.UpdateExistingOutputMessageId.GetValueOrThrow(),
                                converter.GetReplyMarkup(output),
                                output.CallbackQueryId);
                    }
                
                    async Task<TlgMessageId> InvokeSendAttachmentAsync(AttachmentDetails details)
                    {
                        var (blobData, fileName) =
                            await blobLoader.DownloadBlobAsync(details.AttachmentUri);
                        var fileStream = new InputFileStream(blobData, fileName);
                    
                        var caption = details.Caption.Match(
                            static value => value,
                            Option<string>.None);

                        var attachmentSendOutParams = new AttachmentSendOutParameters(
                            outputChatId,
                            fileStream,
                            caption,
                            converter.GetReplyMarkup(output)
                        );

                        return details.AttachmentType switch
                        {
                            TlgAttachmentType.Document => 
                                await outputBotClient.SendDocumentAsync(attachmentSendOutParams),
                            TlgAttachmentType.Photo => 
                                await outputBotClient.SendPhotoAsync(attachmentSendOutParams),
                            TlgAttachmentType.Voice => 
                                await outputBotClient.SendVoiceAsync(attachmentSendOutParams),
                            _ => 
                                throw new ArgumentOutOfRangeException(nameof(details.AttachmentType))
                        };
                    }

                    async Task<TlgMessageId> InvokeSendLocationAsync()
                    {
                        return await outputBotClient
                            .SendLocationAsync(
                                outputChatId,
                                output.Location.GetValueOrThrow(),
                                converter.GetReplyMarkup(output));
                    }
                }

                return sentOutputs;
            };

        var outputGroups = outputs.GroupBy(static o => o.LogicalPort);

        var parallelTasks = outputGroups
            .Select(outputsPerLogicalPortGroup => 
                sendOutputsInSeriesAndOriginalOrder
                    .Invoke(outputsPerLogicalPortGroup
                        .ToArray()));
        
        /* 1) Waits for all parallel executing tasks (generated by .Select()), to complete
         * 2) The 'await' unwraps the resulting aggregate Task object and rethrows any Exceptions */
        return (await Task.WhenAll(parallelTasks))
            .SelectMany(static x => x)
            .ToImmutableArray();
    }
}