using CheckMade.Common.Domain.Data.ChatBot;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.Core.Submissions;

public sealed record SubmissionEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
}