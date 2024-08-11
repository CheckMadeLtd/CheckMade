using CheckMade.Common.Model.Core.Issues.Concrete;

namespace CheckMade.Common.Model.Core.Issues;

public interface ITradeIssueWithEvidence : IIssue
{
    IssueEvidence Evidence { get; }
}