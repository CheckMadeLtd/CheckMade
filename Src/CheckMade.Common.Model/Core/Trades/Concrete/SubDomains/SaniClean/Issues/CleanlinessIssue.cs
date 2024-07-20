using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades.Concrete.Types;

namespace CheckMade.Common.Model.Core.Trades.Concrete.SubDomains.SaniClean.Issues;

public record CleanlinessIssue(
        DateTime CreationDate,
        ISphereOfAction Sphere,
        Option<ITradeFacility<SaniCleanTrade>> Facility,
        Geo Location,
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