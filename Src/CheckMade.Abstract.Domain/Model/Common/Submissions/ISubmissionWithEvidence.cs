namespace CheckMade.Abstract.Domain.Model.Common.Submissions;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}