using CheckMade.Common.DomainModel.Data.Core.Submissions;

namespace CheckMade.Common.DomainModel.Interfaces.Data.Core;

public interface ISubmissionWithEvidence : ISubmission
{
    SubmissionEvidence Evidence { get; }
}