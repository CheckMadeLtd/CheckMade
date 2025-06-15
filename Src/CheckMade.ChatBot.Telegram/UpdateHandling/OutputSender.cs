using System.Collections.Immutable;
using CheckMade.ChatBot.Telegram.BotClient;
using CheckMade.ChatBot.Telegram.Conversion;
using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.Domain.Data.ChatBot.Output;
using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Interfaces.ChatBot.Function;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace CheckMade.ChatBot.Telegram.UpdateHandling;

internal static class OutputSender
{
    internal static async Task<IReadOnlyCollection<Result<OutputDto>>> SendOutputsAsync(
        IReadOnlyCollection<OutputDto> outputs,
        IDictionary<InteractionMode, IBotClientWrapper> botClientByMode,
        TlgAgent currentTlgAgent,
        IReadOnlyCollection<TlgAgentRoleBind> activeRoleBindings,
        IUiTranslator uiTranslator,
        IOutputToReplyMarkupConverter converter,
        IBlobLoader blobLoader,
        ILastOutputMessageIdCache msgIdCache,
        IDomainGlossary glossary,
        ILogger logger)
    {
        // "Bound Ports" are those LogicalPorts where an actual TlgAgent has a binding to the Role and
        // InteractionMode specified in the LogicalPort (i.e. only 'logged in' users).  
        
        Func<TlgAgentRoleBind, LogicalPort, bool> hasBinding = static (tarb, lp) =>
            tarb.Role.Equals(lp.Role) &&
            tarb.TlgAgent.Mode == lp.InteractionMode;
        
        Func<IReadOnlyCollection<OutputDto>, Task<IReadOnlyCollection<Result<OutputDto>>>> 
            sendOutputsInSeriesAndOriginalOrder = async outputsPerBoundPort =>
            {
                List<Result<OutputDto>> sentOutputs = [];
                
                foreach (var output in outputsPerBoundPort)
                {
                    // LogicalPort is only set for outputs aimed at anyone who is NOT the originator of the current input
                    var outputTlgAgent = output.LogicalPort.Match(
                        logicalPort => activeRoleBindings
                            .First(tarb => hasBinding(tarb, logicalPort))
                            .TlgAgent,
                        () => currentTlgAgent);

                    var outputBotClient = botClientByMode[outputTlgAgent.Mode];
                    
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
                            sentOutputs.Add(
                                GetOutputEnrichedWithActualSendOutParams(
                                    await InvokeEditTextMessageAsync()));
                            break;
                    
                        case { Attachments.IsSome: true }:

                            /* Yes, this will only add the id of the last message/attachment that was sent out.
                               The others don't get saved, which is ok for now, because it's only used to determine
                               the destination ids for WorkflowBridges, which would typically not be an attachment
                               message but even if it is, it would make sense for it to be the last one. */
                            
                            Result<TlgMessageId>? messageId = null;
                            
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

                    sentOutputs.Last()
                        .Tap(lastSentOutput => 
                        {
                            if (lastSentOutput.IsSuccess)
                            {
                                msgIdCache.UpdateLastMessageId(
                                    outputTlgAgent,
                                    lastSentOutput.GetValueOrThrow().ActualSendOutParams!.Value.TlgMessageId);
                            }
                            else
                            {
                                logger.LogWarning($"Failure while sending an Output: " +
                                                  $"{lastSentOutput.GetEnglishFailureMessageIfAny()
                                                      .GetValueOrDefault()}");
                            }
                        });

                    continue;

                    Result<OutputDto> GetOutputEnrichedWithActualSendOutParams(Result<TlgMessageId> messageId) =>
                        messageId.Match(
                            id => output with
                            {
                                ActualSendOutParams = new ActualSendOutParams
                                {
                                    TlgMessageId = id,
                                    ChatId = outputTlgAgent.ChatId
                                }
                            },
                            Result<OutputDto>.Fail);
                    
                    async Task<Result<TlgMessageId>> InvokeSendTextMessageAsync()
                    {
                        return await Result<TlgMessageId>.RunAsync(() => 
                            outputBotClient
                                .SendTextMessageAsync(
                                    outputTlgAgent.ChatId.Id,
                                    uiTranslator.Translate(Ui("Please choose:")),
                                    uiTranslator.Translate(output.Text.GetValueOrThrow()),
                                    converter.GetReplyMarkup(output)));
                    }

                    async Task<Result<TlgMessageId>> InvokeEditTextMessageAsync()
                    {
                        return await Result<TlgMessageId>.RunAsync(() =>
                            outputBotClient
                                .EditTextMessageAsync(
                                    outputTlgAgent.ChatId.Id,
                                    output.Text.IsSome
                                        ? uiTranslator.Translate(output.Text.GetValueOrThrow())
                                        : Option<string>.None(),
                                    output.UpdateExistingOutputMessageId.GetValueOrThrow(),
                                    converter.GetReplyMarkup(output),
                                    output.CallbackQueryId));
                    }
                
                    async Task<Result<TlgMessageId>> InvokeSendAttachmentAsync(AttachmentDetails details)
                    {
                        return await (
                            from download 
                                in Result<(MemoryStream, string)>.RunAsync(() => 
                                    blobLoader.DownloadBlobAsync(details.AttachmentUri))
                            from fileStream 
                                in Result<InputFileStream>.Succeed(
                                    new InputFileStream(download.Item1, download.Item2)) 
                            from attachmentSendOutParams
                                in Result<AttachmentSendOutParameters>.Run(() =>
                                    new AttachmentSendOutParameters(
                                        outputTlgAgent.ChatId.Id,
                                        fileStream,
                                        details.Caption,
                                        converter.GetReplyMarkup(output)))
                            from messageId
                                in Result<TlgMessageId>.RunAsync(() =>
                                    details.AttachmentType switch
                                    {
                                        TlgAttachmentType.Document => 
                                            outputBotClient.SendDocumentAsync(attachmentSendOutParams),
                                        TlgAttachmentType.Photo => 
                                            outputBotClient.SendPhotoAsync(attachmentSendOutParams),
                                        TlgAttachmentType.Voice => 
                                            outputBotClient.SendVoiceAsync(attachmentSendOutParams),
                                        _ => 
                                            throw new ArgumentOutOfRangeException(nameof(details.AttachmentType))
                                    })
                            select messageId);
                    }

                    async Task<Result<TlgMessageId>> InvokeSendLocationAsync()
                    {
                        return await Result<TlgMessageId>.RunAsync(() =>
                            outputBotClient
                                .SendLocationAsync(
                                    outputTlgAgent.ChatId.Id,
                                    output.Location.GetValueOrThrow(),
                                    converter.GetReplyMarkup(output)));
                    }
                }
                
                return sentOutputs;
            };
        
        Func<OutputDto, bool> logicalPortIsBound = o => 
            o.LogicalPort.Match(
                logicalPort => activeRoleBindings
                    .Any(tarb => hasBinding(tarb, logicalPort)),
                static () => true
            );

        var outputsPerBoundPortGroups = 
            outputs
                .Where(logicalPortIsBound)
                .GroupBy(static o => o.LogicalPort);

        var parallelTasks = outputsPerBoundPortGroups
            .Select(group => 
                sendOutputsInSeriesAndOriginalOrder
                    .Invoke(group.ToArray()));
        
        LogWarningsForPortsUnboundDueOnlyToMissingMode();
        
        /* 1) Waits for all parallel executing tasks (generated by .Select()), to complete
         * 2) The 'await' unwraps the resulting aggregate Task object and rethrows any Exceptions */
        return (await Task.WhenAll(parallelTasks))
            .SelectMany(static x => x)
            .ToImmutableArray();

        void LogWarningsForPortsUnboundDueOnlyToMissingMode()
        {
            Action<IRoleInfo, InteractionMode> logWarning = (unboundRole, mode) =>
                logger.LogWarning(
                    $"One of the {nameof(outputs)} couldn't be sent due to an unbound {nameof(LogicalPort)}: " +
                    $"No user with the role {glossary.GetUi(unboundRole.RoleType.GetType()).GetFormattedEnglish()} " +
                    $"has a {nameof(TlgAgentRoleBind)} (i.e. is logged in) " +
                    $"for the {mode} bot, even though they are logged in to at least one bot in another mode. " +
                    $"This might not be what the user intended and could reflect a usability problem.");

            Func<IRoleInfo, bool> isBoundForAtLeastOneMode = role =>
                activeRoleBindings.Any(tarb => tarb.Role.Equals(role));
        
            outputs
                .Where(o => !logicalPortIsBound(o))
                .Where(o => isBoundForAtLeastOneMode(o.LogicalPort.GetValueOrThrow().Role))
                .Select(static o => o.LogicalPort.GetValueOrThrow())
                .ToList()
                .ForEach(lp => logWarning(lp.Role, lp.InteractionMode));
        }
    }
}