using CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Proactive.Operations.NewSubmission;

internal sealed record NewIssueWorkflow(
    ILiveEventsRepository LiveEventsRepo,
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    ITlgAgentRoleBindingsRepository RoleBindingsRepo,
    IDomainGlossary Glossary)
    : WorkflowBase(WorkflowUtils, Mediator, BridgesRepo, Glossary)
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(TlgInput currentInput)
    {
        var currentRole = currentInput.OriginatorRole.GetValueOrThrow();
        
        if (!currentRole.IsCurrentRoleTradeSpecific())
        {
            return await WorkflowResponse.CreateFromNextStateAsync(
                currentInput, Mediator.Next(typeof(INewIssueTradeSelection)));
        }

        var trade = currentRole.RoleType.GetTradeInstance().GetValueOrThrow();
        
        var lastKnownLocation = await LastKnownLocationAsync(currentInput, WorkflowUtils);

        var sphere = lastKnownLocation.IsSome
            ? await SphereNearCurrentUserAsync(
                currentInput.LiveEventContext.GetValueOrThrow(),
                LiveEventsRepo,
                lastKnownLocation.GetValueOrThrow(), 
                trade,
                currentInput,
                RoleBindingsRepo) 
            : Option<ISphereOfAction>.None();

        return await sphere.Match(
            _ => trade switch
            {
                SanitaryTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewSubmissionSphereConfirmation<SanitaryTrade>))),
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewSubmissionSphereConfirmation<SiteCleanTrade>))),
                _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
            },
            () => trade switch
            {
                SanitaryTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SanitaryTrade>))),
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SiteCleanTrade>))),
                _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
            });
    }
}