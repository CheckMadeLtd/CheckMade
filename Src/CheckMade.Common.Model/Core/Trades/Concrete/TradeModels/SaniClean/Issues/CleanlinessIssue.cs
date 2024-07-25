using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Trades.Concrete.TradeModels.SaniClean.Issues;

public sealed record CleanlinessIssue(
        Guid Id,
        DateTime CreationDate,
        ISphereOfAction Sphere,
        IFacility Facility,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status) 
    : ITradeIssue, ITradeIssueInvolvingFacility, ITradeIssueWithEvidence
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
}