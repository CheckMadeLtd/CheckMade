using CheckMade.Abstract.Domain.Data.Bot;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Data.Core.Submissions;

public sealed record SubmissionEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
}