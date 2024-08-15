namespace CheckMade.Common.Model.Core.Submissions;

public sealed record SubmissionEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
}