using CheckMade.Common.Domain.Data.Core.Submissions;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}