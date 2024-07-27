using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record StaffIssue<T>(
        Guid Id,
        DateTimeOffset CreationDate, 
        ISphereOfAction Sphere, 
        IssueEvidence Evidence, 
        IRoleInfo ReportedBy, 
        Option<IRoleInfo> HandledBy, 
        IssueStatus Status) 
    : ITradeIssue<T>, IIssueWithEvidence where T : ITrade
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
}