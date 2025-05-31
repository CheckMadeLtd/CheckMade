using System.Collections.Immutable;
using CheckMade.ChatBot.Function.Services.BotClient;
using CheckMade.ChatBot.Function.Services.Conversion;
using CheckMade.ChatBot.Logic;
using CheckMade.Common.Interfaces.ExternalServices.AzureServices;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Utils.UiTranslation;
using Microsoft.Extensions.Logging;
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
        IBlobLoader blobLoader,
        ILogger logger)
    {
        Func<TlgAgentRoleBind, LogicalPort, bool> hasBinding = static (tarb, lp) =>
            tarb.Role.Equals(lp.Role) &&
            tarb.TlgAgent.Mode == lp.InteractionMode;
        
        Func<IReadOnlyCollection<OutputDto>, Task<IReadOnlyCollection<OutputDto>>> sendOutputsInSeriesAndOriginalOrder 
            = async outputsPerBoundPort =>
            {
                List<OutputDto> sentOutputs = [];
                
                foreach (var output in outputsPerBoundPort)
                {
                    // LogicalPort is only set for outputs aimed at anyone who is NOT the originator of the current input
                    var outputMode = output.LogicalPort.Match(
                        static logicalPort => logicalPort.InteractionMode,
                        () => currentInteractionMode);

                    var outputBotClient = botClientByMode[outputMode];
                    
                    var outputChatId = output.LogicalPort.Match(
                        logicalPort => activeRoleBindings
                            .First(tarb => hasBinding(tarb, logicalPort))
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

        // "Bound Ports" are those LogicalPorts where an actual TlgAgent has a binding to the Role and
        // InteractionMode specified in the LogicalPort (i.e. only 'logged in' users).  
        
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

        var glossary = new DomainGlossary();
        
        Action<IRoleInfo, InteractionMode> logWarningForPortsUnboundOnlyDueToMode = (unboundRole, mode) =>
            logger.LogWarning(
                $"One of the {nameof(outputs)} couldn't be sent due to an unbound {nameof(LogicalPort)}: " +
                $"No user with the role {glossary.GetUi(unboundRole.RoleType.GetType()).GetFormattedEnglish()} " +
                $"at {nameof(LiveEvent)} has a {nameof(TlgAgentRoleBind)} (i.e. is logged in) " +
                $"for the {mode} bot, even though they are logged in to at least one bot in another mode. " +
                $"This might not be what the user intended and could reflect a usability problem.");

        Func<IRoleInfo, bool> isBoundForAtLeastOneMode = role =>
            activeRoleBindings.Any(tarb => tarb.Role.Equals(role));
        
        outputs
            .Where(o => !logicalPortIsBound(o))
            .Where(o => isBoundForAtLeastOneMode(o.LogicalPort.GetValueOrThrow().Role))
            .Select(static o => o.LogicalPort.GetValueOrThrow())
            .ToList()
            .ForEach(lp => logWarningForPortsUnboundOnlyDueToMode(lp.Role, lp.InteractionMode));
        
        /* 1) Waits for all parallel executing tasks (generated by .Select()), to complete
         * 2) The 'await' unwraps the resulting aggregate Task object and rethrows any Exceptions */
        return (await Task.WhenAll(parallelTasks))
            .SelectMany(static x => x)
            .ToImmutableArray();
    }
}