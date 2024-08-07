using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.States.A_Init;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.ChatBot.Output;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades.Concrete;
using static CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete.Operations.NewIssue;

internal interface INewIssueWorkflow : IWorkflow;

internal sealed record NewIssueWorkflow(
        ILiveEventsRepository LiveEventsRepo,
        IGeneralWorkflowUtils GeneralWorkflowUtils,
        IStateMediator Mediator)
    : INewIssueWorkflow
{
    public async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        var currentRole = currentInput.OriginatorRole.GetValueOrThrow();
        
        var interactiveHistory =
            await GeneralWorkflowUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);

        var lastInput =
            interactiveHistory
                .SkipLast(1) // skip currentInput
                .LastOrDefault();

        if (lastInput is null)
            return await NewIssueWorkflowInitAsync(currentInput, currentRole);

        var currentStateType = 
            await GeneralWorkflowUtils.GetPreviousResultantStateTypeAsync(
                currentInput, 
                IGeneralWorkflowUtils.DistanceFromCurrentWhenRetrievingPreviousWorkflowState);

        if (currentStateType.IsAssignableTo(typeof(IWorkflowStateTerminator)))
        {
            return WorkflowResponse.Create(
                currentInput,
                new OutputDto { Text = IGeneralWorkflowUtils.WorkflowWasCompleted },
                newState: Mediator.Terminate(currentStateType));
        }
        
        var currentState = Mediator.Next(currentStateType); 
        
        return await currentState.GetWorkflowResponseAsync(currentInput);        
    }

    private async Task<Result<WorkflowResponse>> NewIssueWorkflowInitAsync(
        TlgInput currentInput, 
        IRoleInfo currentRole)
    {
        if (!currentRole.IsCurrentRoleTradeSpecific())
        {
            return await WorkflowResponse.CreateFromNextStateAsync(
                currentInput, Mediator.Next(typeof(INewIssueTradeSelection)));
        }

        var trade = currentRole.RoleType.GetTradeInstance().GetValueOrThrow();
        
        var liveEvent = (await LiveEventsRepo.GetAsync(
            currentInput.LiveEventContext.GetValueOrThrow()))!;
        
        var lastKnownLocation = await LastKnownLocationAsync(currentInput, GeneralWorkflowUtils);

        var sphere = lastKnownLocation.IsSome
            ? SphereNearCurrentUser(liveEvent, lastKnownLocation.GetValueOrThrow(), trade)
            : Option<ISphereOfAction>.None();

        return await sphere.Match(
            _ => trade switch
            {
                SaniCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereConfirmation<SaniCleanTrade>))),
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereConfirmation<SiteCleanTrade>))),
                _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
            },
            () => trade switch
            {
                SaniCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SaniCleanTrade>))),
                SiteCleanTrade => WorkflowResponse.CreateFromNextStateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SiteCleanTrade>))),
                _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
            });
    }
}