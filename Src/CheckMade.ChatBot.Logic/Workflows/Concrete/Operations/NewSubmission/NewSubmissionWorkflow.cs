using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.States.A_Init;
using CheckMade.ChatBot.Logic.Workflows.Utils;
using CheckMade.Common.Domain.Data.ChatBot.Input;
using CheckMade.Common.Domain.Data.Core.Trades;
using CheckMade.Common.Domain.Interfaces.ChatBot.Logic;
using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Domain.Interfaces.Persistence.ChatBot;
using CheckMade.Common.Domain.Interfaces.Persistence.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission.NewSubmissionUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewSubmission;

internal sealed record NewSubmissionWorkflow(
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