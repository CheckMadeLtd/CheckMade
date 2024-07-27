using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean.Issues;

public sealed record StaffIssue(
        Guid Id,
        DateTimeOffset CreationDate, 
        ISphereOfAction Sphere, 
        IssueEvidence Evidence, 
        IRoleInfo ReportedBy, 
        Option<IRoleInfo> HandledBy, 
        IssueStatus Status) 
    : ITradeIssue, ITradeIssueWithEvidence
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
}