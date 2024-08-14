using CheckMade.Common.Model.Core.Submissions.Issues.Concrete;

namespace CheckMade.Common.Model.Core.Submissions.Issues;

public interface ITradeIssueWithEvidence : IIssue
{
    IssueEvidence Evidence { get; }
}