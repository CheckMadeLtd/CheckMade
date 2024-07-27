using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.Core.Trades;

public interface ITradeIssue
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    IRoleInfo ReportedBy { get; }
    Option<IRoleInfo> HandledBy { get; }
    IssueStatus Status { get; }
    
    UiString GetSummary();
}

public enum IssueStatus
{
    Drafting = 1,
    Reported = 10,
    HandlingInProgress = 20,
    HandledReviewRequired = 30,
    ReviewedAndRejected = 40,
    Closed = 90
}