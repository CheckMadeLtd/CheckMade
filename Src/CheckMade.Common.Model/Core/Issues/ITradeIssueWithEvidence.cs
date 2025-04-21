namespace CheckMade.Common.Model.Core.Issues;

public interface ITradeIssueWithEvidence : IIssue
{
    IssueEvidence Evidence { get; }
}