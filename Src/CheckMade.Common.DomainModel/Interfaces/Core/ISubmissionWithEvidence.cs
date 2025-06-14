using CheckMade.Common.DomainModel.Core.Submissions;

namespace CheckMade.Common.DomainModel.Interfaces.Core;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}