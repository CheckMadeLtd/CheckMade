using CheckMade.Abstract.Domain.Data.Core.Submissions;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}