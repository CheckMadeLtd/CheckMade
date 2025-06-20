namespace CheckMade.Abstract.Domain.Model.Core.Submissions;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}