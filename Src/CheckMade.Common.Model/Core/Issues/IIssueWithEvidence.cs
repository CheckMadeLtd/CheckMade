namespace CheckMade.Common.Model.Core.Issues;

public interface IIssueWithEvidence : IIssue
{
    IssueEvidence Evidence { get; }
}