using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.ChatBot.Output;

public record OutputDto
{
    public Option<LogicalPort> LogicalPort { get; init; } 
        = Option<LogicalPort>.None();
    
    public Option<UiString> Text { get; init; } 
        = Option<UiString>.None();
    
    public Option<IReadOnlyCollection<DomainTerm>> DomainTermSelection { get; init; } 
        = Option<IReadOnlyCollection<DomainTerm>>.None();
    
    public Option<ControlPrompts> ControlPromptsSelection { get; init; } 
        = Option<ControlPrompts>.None();
    
    public Option<int> EditReplyMarkupOfMessageId { get; init; }
        = Option<int>.None();
    
    public Option<IReadOnlyCollection<string>> PredefinedChoices { get; init; } 
        = Option<IReadOnlyCollection<string>>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; }
        = Option<IReadOnlyCollection<AttachmentDetails>>.None();
    
    public Option<Geo> Location { get; init; }
        = Option<Geo>.None();
}
