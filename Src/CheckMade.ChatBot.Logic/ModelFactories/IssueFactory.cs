using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean.Issues;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean.Issues;

namespace CheckMade.ChatBot.Logic.ModelFactories;

internal interface IIssueFactory
{
    Task<ITradeIssue> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 

internal sealed record IssueFactory(ILiveEventsRepository LiveEventsRepo) : IIssueFactory
{
    public async Task<ITradeIssue> CreateAsync(IReadOnlyCollection<TlgInput> inputs)
    {
        var liveEvent = (await LiveEventsRepo.GetAsync(
            inputs.Last().LiveEventContext.GetValueOrThrow()))!;
        var role = inputs.Last().OriginatorRole.GetValueOrThrow();
        var currentTrade = role.GetCurrentTrade(inputs);
        var allSpheres = 
            NewIssueWorkflow.GetAllTradeSpecificSpheres(liveEvent, currentTrade);
        
        var lastSelectedIssueType =
            inputs
                .Last(i => 
                    i.Details.DomainTerm.IsSome &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                    i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(ITradeIssue)))
                .Details.DomainTerm.GetValueOrThrow()
                .TypeValue!;
        
        ITradeIssue tradeIssue = currentTrade switch
        {
            SaniCleanTrade =>
                lastSelectedIssueType.Name switch
                {
                    nameof(CleanlinessIssue) =>
                        new CleanlinessIssue(
                            Id: Guid.NewGuid(),
                            CreationDate: DateTime.UtcNow, 
                            Sphere: NewIssueWorkflow.GetLastSelectedSphere(inputs, allSpheres),
                            Facility: GetLastSelectedFacility(),
                            Evidence: GetSubmittedEvidence(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(),
                            Status: IssueStatus.Drafting),
            
                    nameof(ConsumablesIssue) =>
                        new ConsumablesIssue(
                            Id: Guid.NewGuid(),
                            CreationDate: DateTime.UtcNow, 
                            Sphere: NewIssueWorkflow.GetLastSelectedSphere(inputs, allSpheres),
                            AffectedItems: GetSelectedConsumablesItems(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(),
                            Status: IssueStatus.Drafting),

                    nameof(StaffIssue) =>
                        new StaffIssue(
                            Id: Guid.NewGuid(),
                            CreationDate: DateTime.UtcNow, 
                            Sphere: NewIssueWorkflow.GetLastSelectedSphere(inputs, allSpheres),
                            Evidence: GetSubmittedEvidence(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(),
                            Status: IssueStatus.Drafting),

                    nameof(TechnicalIssue) =>
                        new TechnicalIssue(
                            Id: Guid.NewGuid(),
                            CreationDate: DateTime.UtcNow, 
                            Sphere: NewIssueWorkflow.GetLastSelectedSphere(inputs, allSpheres),
                            Facility: GetLastSelectedFacility(),
                            Evidence: GetSubmittedEvidence(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(),
                            Status: IssueStatus.Drafting),
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(lastSelectedIssueType)} for {nameof(currentTrade)} " +
                        $"'{currentTrade.GetType().Name}': '{lastSelectedIssueType.Name}'")
                },
            
            SiteCleanTrade =>
                lastSelectedIssueType.Name switch
                {
                    nameof(GeneralSiteCleanIssue) =>
                        new GeneralSiteCleanIssue(
                            Id: Guid.NewGuid(), 
                            CreationDate: DateTime.UtcNow, 
                            Sphere: NewIssueWorkflow.GetLastSelectedSphere(inputs, allSpheres),
                            Evidence: GetSubmittedEvidence(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(), 
                            Status: IssueStatus.Drafting),
                    
                    _ => throw new InvalidOperationException(
                        $"Unhandled {nameof(lastSelectedIssueType)} for {nameof(currentTrade)} " +
                        $"'{currentTrade.GetType().Name}': '{lastSelectedIssueType.Name}'")
                },
            
            _ => throw new InvalidOperationException(
                $"Unhandled {nameof(currentTrade)}: '{currentTrade.GetType().Name}'")
        };

        return tradeIssue;

        IssueEvidence GetSubmittedEvidence()
        {
            // concatenate multiple descriptions into a single string... 
            
        }
        
        IFacility GetLastSelectedFacility()
        {
            var lastFacilityType =
                inputs.LastOrDefault(i =>
                        i.Details.DomainTerm.IsSome &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IFacility)))?
                    .Details.DomainTerm.GetValueOrThrow()
                    .TypeValue;
        }

        IReadOnlyCollection<ConsumablesIssue.Item> GetSelectedConsumablesItems()
        {
            
        }
    }
}