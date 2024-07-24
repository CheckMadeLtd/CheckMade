using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean.Issues;

public record CleanlinessIssue(
        Guid Id,
        DateTime CreationDate,
        ISphereOfAction Sphere,
        ITradeFacility Facility,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status = IssueStatus.Reported) 
    : ITradeIssue, ITradeIssueInvolvingFacility
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
}