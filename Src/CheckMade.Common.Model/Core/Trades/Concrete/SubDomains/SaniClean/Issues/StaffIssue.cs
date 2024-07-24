using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;

public record StaffIssue(
        Guid Id,
        DateTime CreationDate, 
        ISphereOfAction Sphere, 
        IssueEvidence Evidence, 
        IRoleInfo ReportedBy, 
        Option<IRoleInfo> HandledBy, 
        IssueStatus Status = IssueStatus.Reported) 
    : ITradeIssue<SaniCleanTrade>
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
}