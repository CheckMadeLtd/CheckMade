namespace CheckMade.Common.DomainModel.Core.Submissions;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}