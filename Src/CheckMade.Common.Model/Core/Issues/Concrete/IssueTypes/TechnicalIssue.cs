using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record TechnicalIssue<T>(
        Guid Id,    
        DateTimeOffset CreationDate,
        ISphereOfAction Sphere,
        IFacility Facility,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status) 
    : ITradeIssue<T>, IIssueInvolvingFacility, IIssueWithEvidence where T : ITrade
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
}