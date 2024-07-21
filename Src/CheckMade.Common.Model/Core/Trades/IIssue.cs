using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.Trades.Concrete.SubDomains;

namespace CheckMade.Common.Model.Core.Trades;

public interface IIssue
{
    Guid Id { get; }
    DateTime CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Option<Geo> PreciseLocation { get; }
    IssueEvidence Evidence { get; }
    IRoleInfo ReportedBy { get; }
    Option<IRoleInfo> HandledBy { get; }
    IssueStatus Status { get; }
    
    UiString GetSummary();
}

public enum IssueStatus
{
    Reported = 10,
    HandlingInProgress = 20,
    HandledReviewRequired = 30,
    ReviewedAndRejected = 40,
    Closed = 90
}