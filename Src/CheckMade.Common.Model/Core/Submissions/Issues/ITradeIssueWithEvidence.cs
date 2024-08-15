namespace CheckMade.Common.Model.Core.Submissions.Issues;

public interface ITradeIssueWithEvidence : IIssue
{
    SubmissionEvidence Evidence { get; }
}