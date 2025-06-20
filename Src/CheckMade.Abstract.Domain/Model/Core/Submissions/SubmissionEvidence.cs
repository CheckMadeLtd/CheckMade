using CheckMade.Abstract.Domain.Model.Bot.DTOs;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Core.Submissions;

public sealed record SubmissionEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
}