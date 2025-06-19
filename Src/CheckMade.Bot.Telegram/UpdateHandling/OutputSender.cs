using System.Collections.Immutable;
using CheckMade.Abstract.Domain.Data.Bot;
using CheckMade.Abstract.Domain.Data.Bot.Output;
using CheckMade.Abstract.Domain.Data.Bot.UserInteraction;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Function;
using CheckMade.Abstract.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Abstract.Domain.Interfaces.ExternalServices.AzureServices;
using CheckMade.Bot.Telegram.BotClient;
using CheckMade.Bot.Telegram.Conversion;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using MessageId = CheckMade.Abstract.Domain.Data.Bot.MessageId;

namespace CheckMade.Bot.Telegram.UpdateHandling;

internal static class OutputSender
{
    internal static async Task<IReadOnlyCollection<Result<Output>>> SendOutputsAsync(
        IReadOnlyCollection<Output> outputs,
        IDictionary<InteractionMode, IBotClientWrapper> botClientByMode,
        Agent currentAgent,
        IReadOnlyCollection<AgentRoleBind> activeRoleBindings,
        IUiTranslator uiTranslator,
        IOutputToReplyMarkupConverter converter,
        IBlobLoader blobLoader,
        ILastOutputMessageIdCache msgIdCache,
        IDomainGlossary glossary,
        ILogger logger)
    {
        // "Bound Ports" are those LogicalPorts where an actual Agent has a binding to the Role and
        // InteractionMode specified in the LogicalPort (i.e. only 'logged in' users).  
        
        Func<AgentRoleBind, LogicalPort, bool> hasBinding = static (arb, lp) =>
            arb.Role.Equals(lp.Role) &&
            arb.Agent.Mode == lp.InteractionMode;
        
        Func<IReadOnlyCollection<Output>, Task<IReadOnlyCollection<Result<Output>>>> 
            sendOutputsInSeriesAndOriginalOrder = async outputsPerBoundPort =>
            {
                List<Result<Output>> sentOutputs = [];
                
                foreach (var output in outputsPerBoundPort)
                {
                    // LogicalPort is only set for outputs aimed at anyone who is NOT the originator of the current input
                    var outputAgent = output.LogicalPort.Match(
                        logicalPort => activeRoleBindings
                            .First(arb => hasBinding(arb, logicalPort))
                            .Agent,
                        () => currentAgent);

                    var outputBotClient = botClientByMode[outputAgent.Mode];
                    
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
                            
                            Result<MessageId>? messageId = null;
                            
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
                                    outputAgent,
                                    lastSentOutput.GetValueOrThrow().ActualSendOutParams!.Value.MessageId);
                            }
                            else
                            {
                                logger.LogWarning($"Failure while sending an Output: " +
                                                  $"{lastSentOutput.GetEnglishFailureMessageIfAny()
                                                      .GetValueOrDefault()}");
                            }
                        });

                    continue;

                    Result<Output> GetOutputEnrichedWithActualSendOutParams(Result<MessageId> messageId) =>
                        messageId.Match(
                            id => output with
                            {
                                ActualSendOutParams = new ActualSendOutParams
                                {
                                    MessageId = id,
                                    ChatId = outputAgent.ChatId
                                }
                            },
                            Result<Output>.Fail);
                    
                    async Task<Result<MessageId>> InvokeSendTextMessageAsync()
                    {
                        return await Result<MessageId>.RunAsync(() => 
                            outputBotClient
                                .SendTextMessageAsync(
                                    outputAgent.ChatId.Id,
                                    uiTranslator.Translate(Ui("Please choose:")),
                                    uiTranslator.Translate(output.Text.GetValueOrThrow()),
                                    converter.GetReplyMarkup(output)));
                    }

                    async Task<Result<MessageId>> InvokeEditTextMessageAsync()
                    {
                        return await Result<MessageId>.RunAsync(() =>
                            outputBotClient
                                .EditTextMessageAsync(
                                    outputAgent.ChatId.Id,
                                    output.Text.IsSome
                                        ? uiTranslator.Translate(output.Text.GetValueOrThrow())
                                        : Option<string>.None(),
                                    output.UpdateExistingOutputMessageId.GetValueOrThrow(),
                                    converter.GetReplyMarkup(output),
                                    output.CallbackQueryId));
                    }
                
                    async Task<Result<MessageId>> InvokeSendAttachmentAsync(AttachmentDetails details)
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
                                        outputAgent.ChatId.Id,
                                        fileStream,
                                        details.Caption,
                                        converter.GetReplyMarkup(output)))
                            from messageId
                                in Result<MessageId>.RunAsync(() =>
                                    details.AttachmentType switch
                                    {
                                        AttachmentType.Document => 
                                            outputBotClient.SendDocumentAsync(attachmentSendOutParams),
                                        AttachmentType.Photo => 
                                            outputBotClient.SendPhotoAsync(attachmentSendOutParams),
                                        AttachmentType.Voice => 
                                            outputBotClient.SendVoiceAsync(attachmentSendOutParams),
                                        _ => 
                                            throw new ArgumentOutOfRangeException(nameof(details.AttachmentType))
                                    })
                            select messageId);
                    }

                    async Task<Result<MessageId>> InvokeSendLocationAsync()
                    {
                        return await Result<MessageId>.RunAsync(() =>
                            outputBotClient
                                .SendLocationAsync(
                                    outputAgent.ChatId.Id,
                                    output.Location.GetValueOrThrow(),
                                    converter.GetReplyMarkup(output)));
                    }
                }
                
                return sentOutputs;
            };
        
        Func<Output, bool> logicalPortIsBound = o => 
            o.LogicalPort.Match(
                logicalPort => activeRoleBindings
                    .Any(arb => hasBinding(arb, logicalPort)),
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
                    $"has a {nameof(AgentRoleBind)} (i.e. is logged in) " +
                    $"for the {mode} bot, even though they are logged in to at least one bot in another mode. " +
                    $"This might not be what the user intended and could reflect a usability problem.");

            Func<IRoleInfo, bool> isBoundForAtLeastOneMode = role =>
                activeRoleBindings.Any(arb => arb.Role.Equals(role));
        
            outputs
                .Where(o => !logicalPortIsBound(o))
                .Where(o => isBoundForAtLeastOneMode(o.LogicalPort.GetValueOrThrow().Role))
                .Select(static o => o.LogicalPort.GetValueOrThrow())
                .ToList()
                .ForEach(lp => logWarning(lp.Role, lp.InteractionMode));
        }
    }
}