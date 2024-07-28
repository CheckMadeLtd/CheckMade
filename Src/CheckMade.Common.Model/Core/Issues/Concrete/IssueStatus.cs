namespace CheckMade.Common.Model.Core.Issues.Concrete;

public enum IssueStatus
{
    Drafting = 10,
    Reported = 20,
    HandlingInProgress = 30,
    HandledReviewRequired = 40,
    ReviewedAndRejected = 50,
    Closed = 90
}