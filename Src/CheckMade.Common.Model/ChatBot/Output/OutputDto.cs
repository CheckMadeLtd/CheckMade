using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.ChatBot.Output;

public record OutputDto
{
    public Option<LogicalPort> LogicalPort { get; init; } 
        = Option<LogicalPort>.None();
    
    public Option<UiString> Text { get; init; } 
        = Option<UiString>.None();
    
    public Option<IEnumerable<OneOf<int, Type>>> DomainTermSelection { get; init; } 
        = Option<IEnumerable<OneOf<int, Type>>>.None();
    
    public Option<ControlPrompts> ControlPromptsSelection { get; init; } 
        = Option<ControlPrompts>.None();
    
    public Option<IEnumerable<string>> PredefinedChoices { get; init; } 
        = Option<IEnumerable<string>>.None();
    
    public Option<IEnumerable<OutputAttachmentDetails>> Attachments { get; init; }
        = Option<IEnumerable<OutputAttachmentDetails>>.None();
    
    public Option<Geo> Location { get; init; }
        = Option<Geo>.None();
}
