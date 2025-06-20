using CheckMade.Core.Model.Bot.DTOs;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Common.Submissions;

public sealed record SubmissionEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
}