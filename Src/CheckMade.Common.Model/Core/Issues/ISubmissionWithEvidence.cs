namespace CheckMade.Common.Model.Core.Issues;

public interface ISubmissionWithEvidence : ISubmission
{
    IssueEvidence Evidence { get; }
}