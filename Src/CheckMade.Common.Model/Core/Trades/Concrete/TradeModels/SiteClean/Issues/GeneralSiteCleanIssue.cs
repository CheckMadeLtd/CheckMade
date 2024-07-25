using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SiteClean.Issues;

public sealed record GeneralSiteCleanIssue(
        Guid Id, 
        DateTime CreationDate, 
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