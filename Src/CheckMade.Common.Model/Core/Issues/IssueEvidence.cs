namespace CheckMade.Common.Model.Core.Issues;

public sealed record IssueEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
}