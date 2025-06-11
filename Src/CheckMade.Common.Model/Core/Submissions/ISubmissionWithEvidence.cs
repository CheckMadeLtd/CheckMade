namespace CheckMade.Common.Model.Core.Submissions;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}