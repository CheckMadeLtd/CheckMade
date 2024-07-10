namespace CheckMade.Common.Model.Core.Trades.Concrete.SubDomains;

public record IssueEvidence
{
    public Option<string> Description { get; init; } = 
        Option<string>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Media { get; init; } =
        Option<IReadOnlyCollection<AttachmentDetails>>.None();
}