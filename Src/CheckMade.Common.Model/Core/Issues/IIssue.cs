using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Issues;

public interface IIssue
{
    Guid Id { get; }
    DateTimeOffset CreationDate { get; }
    ISphereOfAction Sphere { get; }
    IRoleInfo ReportedBy { get; }
    Option<IRoleInfo> HandledBy { get; }
    IssueStatus Status { get; }
    IDomainGlossary Glossary { get; }
    
    UiString FormatDetails();
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