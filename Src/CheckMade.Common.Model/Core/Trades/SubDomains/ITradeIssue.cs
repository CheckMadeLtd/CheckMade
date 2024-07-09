using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Core.Trades.SubDomains;

public interface ITradeIssue<T> where T : ITrade
{
    DateTime CreationDate { get; }
    ISphereOfAction Sphere { get; }
    Option<ITradeFacility<T>> Facility { get; }
    Geo Location { get; }
    IssueEvidence Evidence { get; }
    IRoleInfo ReportedBy { get; }
    Option<IRoleInfo> HandledBy { get; }
    IssueStatus Status { get; }
}

public enum IssueStatus
{
    Reported = 10,
    HandlingInProgress = 20,
    HandledReviewRequired = 30,
    ReviewedAndRejected = 40,
    Closed = 90
}