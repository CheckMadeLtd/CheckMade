namespace CheckMade.Core.Model.Common.Submissions;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}