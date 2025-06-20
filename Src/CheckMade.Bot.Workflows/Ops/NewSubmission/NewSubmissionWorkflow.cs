using CheckMade.Core.Model.Bot.DTOs.Input;
using CheckMade.Core.Model.Common.LiveEvents;
using CheckMade.Core.Model.Common.Trades;
using CheckMade.Core.ServiceInterfaces.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Bot;
using CheckMade.Core.ServiceInterfaces.Persistence.Common;
using CheckMade.Bot.Workflows.Ops.NewSubmission.States.A_Init;
using CheckMade.Bot.Workflows.Utils;
using General.Utils.FpExtensions.Monads;
using static CheckMade.Bot.Workflows.Ops.NewSubmission.NewSubmissionUtils;

namespace CheckMade.Bot.Workflows.Ops.NewSubmission;

public sealed record NewSubmissionWorkflow(
    ILiveEventsRepository LiveEventsRepo,
    IGeneralWorkflowUtils WorkflowUtils,
    IStateMediator Mediator,
    IDerivedWorkflowBridgesRepository BridgesRepo,
    IAgentRoleBindingsRepository RoleBindingsRepo,
    IDomainGlossary Glossary)
    : WorkflowBase(WorkflowUtils, Mediator, BridgesRepo, Glossary)
{
    protected override async Task<Result<WorkflowResponse>> InitializeAsync(Input currentInput)
    {
        var currentRole = currentInput.OriginatorRole.GetValueOrThrow();
        
        if (!currentRole.IsCurrentRoleTradeSpecific())
        {
            return await WorkflowResponse.CreateFromNextStateAsync(
                currentInput, Mediator.Next(typeof(INewSubmissionTradeSelection)));
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
                    currentInput, Mediator.Next(typeof(INewSubmissionSphereSelection<SanitaryTrade>))),
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewSubmissionSphereSelection<SiteCleanTrade>))),
                _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
            });
    }
}