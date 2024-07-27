using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

using static ConsumablesIssue;

public sealed record ConsumablesIssue(
        Guid Id,
        DateTimeOffset CreationDate,
        ISphereOfAction Sphere,
        IReadOnlyCollection<Item> AffectedItems,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status) 
    : ITradeIssue
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
    
    public enum Item
    {
        ToiletPaper,
        PaperTowels,
        Soap,
    }
}