using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Issues.Concrete.IssueTypes;

public sealed record TechnicalIssue(
        Guid Id,    
        DateTimeOffset CreationDate,
        ISphereOfAction Sphere,
        IFacility Facility,
        IssueEvidence Evidence,
        IRoleInfo ReportedBy,
        Option<IRoleInfo> HandledBy,
        IssueStatus Status) 
    : IIssue, IIssueInvolvingFacility, IIssueWithEvidence
{
    public UiString GetSummary()
    {
        throw new NotImplementedException();
    }
}