using System.Text;
using CheckMade.ChatBot.Logic.Utils;
using CheckMade.ChatBot.Logic.Workflows.Concrete.NewIssueStates;
using CheckMade.Common.Interfaces.ChatBot.Logic;
using CheckMade.Common.Interfaces.Persistence.Core;
using CheckMade.Common.Model;
using CheckMade.Common.Model.ChatBot.Input;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean.Issues;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean;
using CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean.Issues;
using static CheckMade.ChatBot.Logic.Utils.NewIssueUtils;

namespace CheckMade.ChatBot.Logic.ModelFactories;

internal interface IIssueFactory
{
    Task<ITradeIssue> CreateAsync(IReadOnlyCollection<TlgInput> inputs);
} 

internal sealed record IssueFactory(
        ILiveEventsRepository LiveEventsRepo,
        IDomainGlossary Glossary) 
    : IIssueFactory
{
    public async Task<ITradeIssue> CreateAsync(IReadOnlyCollection<TlgInput> inputs)
    {
        var liveEvent = (await LiveEventsRepo.GetAsync(
            inputs.Last().LiveEventContext.GetValueOrThrow()))!;
        var role = inputs.Last().OriginatorRole.GetValueOrThrow();
        var currentTrade = role.GetCurrentTrade(inputs);
        var allSpheres = 
            GetAllTradeSpecificSpheres(liveEvent, currentTrade);
        
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
                            Sphere: GetLastSelectedSphere(inputs, allSpheres),
                            Facility: GetLastSelectedFacility(),
                            Evidence: GetSubmittedEvidence(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(),
                            Status: IssueStatus.Drafting),
            
                    nameof(ConsumablesIssue) =>
                        new ConsumablesIssue(
                            Id: Guid.NewGuid(),
                            CreationDate: DateTime.UtcNow, 
                            Sphere: GetLastSelectedSphere(inputs, allSpheres),
                            AffectedItems: GetSelectedConsumablesItems(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(),
                            Status: IssueStatus.Drafting),

                    nameof(StaffIssue) =>
                        new StaffIssue(
                            Id: Guid.NewGuid(),
                            CreationDate: DateTime.UtcNow, 
                            Sphere: GetLastSelectedSphere(inputs, allSpheres),
                            Evidence: GetSubmittedEvidence(),
                            ReportedBy: role,
                            HandledBy: Option<IRoleInfo>.None(),
                            Status: IssueStatus.Drafting),

                    nameof(TechnicalIssue) =>
                        new TechnicalIssue(
                            Id: Guid.NewGuid(),
                            CreationDate: DateTime.UtcNow, 
                            Sphere: GetLastSelectedSphere(inputs, allSpheres),
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
                            Sphere: GetLastSelectedSphere(inputs, allSpheres),
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
            var combinedDescriptionEvidence = new StringBuilder();

            var submittedDescriptions = 
                inputs
                    .Where(i =>
                        i.InputType == TlgInputType.TextMessage &&
                        i.ResultantWorkflow.IsSome &&
                        i.ResultantWorkflow.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewIssueEvidenceEntry<ITrade>)))
                    .Select(i => i.Details.Text.GetValueOrThrow())
                    .ToImmutableReadOnlyCollection();

            foreach (var text in submittedDescriptions)
            {
                combinedDescriptionEvidence.Append(text);
            }

            var submittedMedia =
                inputs
                    .Where(i =>
                        i.InputType == TlgInputType.AttachmentMessage &&
                        i.ResultantWorkflow.IsSome &&
                        i.ResultantWorkflow.GetValueOrThrow().InStateId ==
                        Glossary.GetId(typeof(INewIssueEvidenceEntry<ITrade>)))
                    .ToImmutableReadOnlyCollection();

            List<AttachmentDetails> mediaEvidence = [];
            
            mediaEvidence
                .AddRange(submittedMedia
                    .Select(attachment => 
                        new AttachmentDetails(
                            attachment.Details.AttachmentInternalUri.GetValueOrThrow(),
                            attachment.Details.AttachmentType.GetValueOrThrow(),
                            attachment.Details.Text)));

            return new IssueEvidence
            {
                Description = combinedDescriptionEvidence.ToString(),
                Media = mediaEvidence
            };
        }
        
        IFacility GetLastSelectedFacility()
        {
            var lastFacilityType =
                inputs.LastOrDefault(i =>
                        i.Details.DomainTerm.IsSome &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue != null &&
                        i.Details.DomainTerm.GetValueOrThrow().TypeValue!.IsAssignableTo(typeof(IFacility)))?
                    .Details.DomainTerm.GetValueOrThrow()
                    .TypeValue!;

            try
            {
                return (IFacility)Activator.CreateInstance(lastFacilityType)!;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Every subtype of {nameof(IFacility)} requires a parameterless constructor." +
                    $"This allows for usage of '{nameof(Activator)}.{nameof(Activator.CreateInstance)}' " +
                    $"instead of using a switch expression switching on the given {nameof(lastFacilityType)}. " +
                    $"We thereby reduce maintenance when new subtypes are added", ex);
            }
        }

        IReadOnlyCollection<ConsumablesIssue.Item> GetSelectedConsumablesItems()
        {
            return Glossary.GetAll(typeof(ConsumablesIssue.Item))
                .Where(dt => dt.IsToggleOn(inputs))
                .Select(dt => (ConsumablesIssue.Item)dt.EnumValue!)
                .ToImmutableReadOnlyCollection();
        }
    }
}