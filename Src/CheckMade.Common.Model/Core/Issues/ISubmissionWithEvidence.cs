namespace CheckMade.Common.Model.Core.Issues;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}