using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.Submissions.Issues.Concrete;

namespace CheckMade.Common.Model.Core.Submissions.Issues;

public interface IIssue : ISubmission
{
    Option<Role> HandledBy { get; }
    IssueStatus Status { get; }
    
    IReadOnlyDictionary<IssueSummaryCategories, UiString> GetSummary();
}
