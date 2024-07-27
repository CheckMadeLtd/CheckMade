namespace CheckMade.Common.Model.Core.Issues.Concrete;

public sealed record IssueEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Media { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
    
    
}