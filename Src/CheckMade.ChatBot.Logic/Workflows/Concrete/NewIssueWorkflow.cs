using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;
using CheckMade.Common.Utils.GIS;

namespace CheckMade.ChatBot.Logic.Workflows.Concrete;

internal interface INewIssueWorkflow : IWorkflow
{
    Task<IIssue> ConstructIssueAsync(IReadOnlyCollection<TlgInput> inputs);
}

internal record NewIssueWorkflow(
        ILiveEventsRepository LiveEventsRepo,
        ILogicUtils LogicUtils,
        IStateMediator Mediator)
    : INewIssueWorkflow
{
    public bool IsCompleted(IReadOnlyCollection<TlgInput> inputHistory)
    {
        // ToDo: implement correctly once we have entire workflow.
        return false;
    }

    public async Task<Result<WorkflowResponse>> GetResponseAsync(TlgInput currentInput)
    {
        var currentRole = currentInput.OriginatorRole.GetValueOrThrow();
        
        var interactiveHistory =
            await LogicUtils.GetInteractiveSinceLastBotCommandAsync(currentInput);

        var lastInput =
            interactiveHistory
                .SkipLast(1) // skip currentInput
                .LastOrDefault();

        if (lastInput is null)
            return await NewIssueWorkflowInitAsync(currentInput, currentRole);

        var currentStateType = 
            await LogicUtils.GetPreviousStateTypeAsync(
                currentInput, 
                ILogicUtils.DistanceFromCurrentWhenRetrievingPreviousWorkflowState);

        var currentState = Mediator.Next(currentStateType); 
        
        return await currentState.GetWorkflowResponseAsync(currentInput);        
    }

    private async Task<Result<WorkflowResponse>> NewIssueWorkflowInitAsync(
        TlgInput currentInput, 
        IRoleInfo currentRole)
    {
        if (!currentRole.IsCurrentRoleTradeSpecific())
        {
            return await WorkflowResponse.CreateAsync(
                currentInput, Mediator.Next(typeof(INewIssueTradeSelection)));
        }

        var trade = currentRole.RoleType.GetTradeInstance().GetValueOrThrow();
        
        var liveEvent = (await LiveEventsRepo.GetAsync(
            currentInput.LiveEventContext.GetValueOrThrow()))!;
        
        var lastKnownLocation = await LastKnownLocationAsync(currentInput, LogicUtils);

        var sphere = lastKnownLocation.IsSome
            ? SphereNearCurrentUser(liveEvent, lastKnownLocation.GetValueOrThrow(), trade)
            : Option<ISphereOfAction>.None();

        return await sphere.Match(
            _ => trade switch
            {
                SaniCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereConfirmation<SaniCleanTrade>))),
                SiteCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereConfirmation<SiteCleanTrade>))),
                _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
            },
            () => trade switch
            {
                SaniCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SaniCleanTrade>))),
                SiteCleanTrade => WorkflowResponse.CreateAsync(
                    currentInput, Mediator.Next(typeof(INewIssueSphereSelection<SiteCleanTrade>))),
                _ => throw new InvalidOperationException($"Unhandled {nameof(trade)}: '{trade}'")
            });
    }

    internal static async Task<Option<Geo>> LastKnownLocationAsync(
        TlgInput currentInput, ILogicUtils logicUtils)
    {
        var lastKnownLocationInput =
            (await logicUtils.GetRecentLocationHistory(currentInput.TlgAgent))
            .LastOrDefault();

        return lastKnownLocationInput is null 
            ? Option<Geo>.None() 
            : lastKnownLocationInput.Details.GeoCoordinates.GetValueOrThrow();
    }
    
    internal static Option<ISphereOfAction> SphereNearCurrentUser(
        LiveEvent liveEvent,
        Geo lastKnownLocation,
        ITrade trade)
    {
        var tradeSpecificNearnessThreshold = trade switch
        {
            SaniCleanTrade => SaniCleanTrade.SphereNearnessThresholdInMeters,
            SiteCleanTrade => SiteCleanTrade.SphereNearnessThresholdInMeters,
            _ => throw new InvalidOperationException("Missing switch for ITrade type for nearness threshold")
        };

        var allSpheres =
            GetAllTradeSpecificSpheres(
                liveEvent ?? throw new InvalidOperationException("LiveEvent missing."),
                trade);

        var nearSphere =
            allSpheres
                .Where(soa =>
                    DistanceFromLastKnownLocation(soa) < tradeSpecificNearnessThreshold)
                .MinBy(DistanceFromLastKnownLocation);

        return
            nearSphere != null
                ? Option<ISphereOfAction>.Some(nearSphere)
                : Option<ISphereOfAction>.None();

        double DistanceFromLastKnownLocation(ISphereOfAction soa) =>
            soa.Details.GeoCoordinates.GetValueOrThrow()
                .MetersAwayFrom(lastKnownLocation);
    }
    
    private static IReadOnlyCollection<ISphereOfAction>
        GetAllTradeSpecificSpheres(LiveEvent liveEvent, ITrade trade) =>
        liveEvent
            .DivIntoSpheres
            .Where(soa => soa.GetTradeType() == trade.GetType())
            .ToImmutableReadOnlyCollection();
    
    public async Task<IIssue> ConstructIssueAsync(IReadOnlyCollection<TlgInput> inputs)
    {
        var role = inputs.Last().OriginatorRole.GetValueOrThrow();
        var liveEventInfo = inputs.Last().LiveEventContext.GetValueOrThrow();
        var liveEvent = (await LiveEventsRepo.GetAsync(liveEventInfo))!;
        var trade = role.GetCurrentTrade(inputs);
        var spheres = GetAllTradeSpecificSpheres(liveEvent, trade);
        var guid = Guid.NewGuid();
        
        // var lastFacilityType =
        //     inputs.LastOrDefault(i =>
        //         i.Details.DomainTerm.IsSome &&
        //         i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
        //         i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IFacility)))?
        //         .Details.DomainTerm.GetValueOrThrow()
        //         .TypeValue;

        var evidence = new IssueEvidence();
                
        // ToDo: construct a new IssueEvidence based on submitted descriptions and media
        // concatenate multiple descriptions into a single string... 
        
        var lastSelectedIssueType =
            inputs
                .Last(i => 
                    i.Details.DomainTerm.IsSome &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IIssue)))
                .Details.DomainTerm.GetValueOrThrow()
                .TypeValue!;

        IIssue issue = lastSelectedIssueType.Name switch
        {
            nameof(CleanlinessIssue) =>
                new CleanlinessIssue(
                    Id: guid,
                    CreationDate: DateTime.UtcNow, 
                    Sphere: GetLastSelectedSphere(),
                    Facility: lastSelectedFacility, // use a Func<inputs, ITradeFacility<SaniCleanTrade>>
                    Evidence: evidence,
                    ReportedBy: role,
                    HandledBy: Option<IRoleInfo>.None()),
            
            nameof(TechnicalIssue) =>
                new TechnicalIssue(
                    Id: guid,
                    CreationDate: DateTime.UtcNow, 
                    Sphere: GetLastSelectedSphere(),
                    Evidence: evidence,
                    ReportedBy: role,
                    HandledBy: Option<IRoleInfo>.None()),
            
            nameof(ConsumablesIssue) =>
                new ConsumablesIssue(
                    Id: guid,
                    CreationDate: DateTime.UtcNow, 
                    Sphere: GetLastSelectedSphere(),
                    Facility: lastSelectedFacility, // use delegate, constitute facility incl. affected consumables from input history
                    Evidence: evidence,
                    ReportedBy: role,
                    HandledBy: Option<IRoleInfo>.None()),
        };
        
        // return new CleanlinessIssue();
        throw new NotImplementedException();

        ISphereOfAction GetLastSelectedSphere()
        {
            var lastSelectedSphereName =
                inputs.Last(i =>
                        i.Details.Text.IsSome &&
                        spheres.Select(s => s.Name)
                            .Contains(i.Details.Text.GetValueOrThrow()))
                    .Details.Text.GetValueOrThrow();

            return
                spheres
                    .First(s => 
                        s.Name.Equals(lastSelectedSphereName));
        }
        
        
    }
}