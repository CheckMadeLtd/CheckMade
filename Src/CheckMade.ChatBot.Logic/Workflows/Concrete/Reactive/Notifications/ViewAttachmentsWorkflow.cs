using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewIssue.States.D_Terminators;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.ChatBotLogic;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Issues;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Reactive.Notifications;

internal sealed record ViewAttachmentsWorkflow(
    IGeneralWorkflowUtils WorkflowUtils,   
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    ITlgInputsRepository InputsRepo,
    IServiceProvider Services,
    IDomainGlossary Glossary) 
    : WorkflowBase(WorkflowUtils, Mediator, BridgesRepo, Glossary)
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        var workflowBridge =
            await BridgesRepo.GetAsync(
                currentInput.TlgAgent.ChatId, currentInput.TlgMessageId);

        if (workflowBridge is null)
            throw new InvalidOperationException(
                $"It shouldn't be possible to enter the {nameof(ViewAttachmentsWorkflow)} " +
                $"without a {nameof(workflowBridge)}");

        var issueHistory = 
            await InputsRepo.GetEntityHistoryAsync(
                currentInput.LiveEventContext.GetValueOrThrow(), 
                workflowBridge.SourceInput.EntityGuid.GetValueOrThrow());

        var sourceWorkflowTerminator = Mediator.GetTerminator(
            Glossary.GetDtType(
                workflowBridge
                    .SourceInput.ResultantState.GetValueOrThrow()
                    .InStateId));

        var issue = sourceWorkflowTerminator switch
        {
            INewIssueSubmissionSucceeded<SanitaryTrade> =>
                (IIssueWithEvidence)
                await Services.GetRequiredService<IIssueFactory<SanitaryTrade>>()
                    .CreateAsync(issueHistory),

            INewIssueSubmissionSucceeded<SiteCleanTrade> =>
                (IIssueWithEvidence)
                await Services.GetRequiredService<IIssueFactory<SiteCleanTrade>>()
                    .CreateAsync(issueHistory),

            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(sourceWorkflowTerminator)} while attempting to resolve {nameof(IIssueFactory<ITrade>)}")
        };

        var attachmentsOutput = new OutputDto
        {
            Attachments = Option<IReadOnlyCollection<AttachmentDetails>>.Some(
                issue.Evidence.Attachments.GetValueOrThrow())
        };

        return WorkflowResponse.Create(
            currentInput,
            attachmentsOutput,
            newState: Mediator.GetTerminator(typeof(IOneStepWorkflowTerminator)),
            promptTransition: new PromptTransition(currentInput.TlgMessageId));
    }
}