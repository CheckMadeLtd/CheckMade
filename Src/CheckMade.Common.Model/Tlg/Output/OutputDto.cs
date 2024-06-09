using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.UserInteraction;

namespace CheckMade.Common.Model.Tlg.Output;

public record OutputDto
{
    public Option<TlgLogicPort> LogicalPort { get; init; } 
        = Option<TlgLogicPort>.None();
    
    public Option<UiString> Text { get; init; } 
        = Option<UiString>.None();
    
    public Option<IEnumerable<DomainCategory>> DomainCategorySelection { get; init; } 
        = Option<IEnumerable<DomainCategory>>.None();
    
    public Option<IEnumerable<ControlPrompts>> ControlPromptsSelection { get; init; } 
        = Option<IEnumerable<ControlPrompts>>.None();
    
    public Option<IEnumerable<string>> PredefinedChoices { get; init; } 
        = Option<IEnumerable<string>>.None();
    
    public Option<IEnumerable<OutputAttachmentDetails>> Attachments { get; init; }
        = Option<IEnumerable<OutputAttachmentDetails>>.None();
    
    public Option<Geo> Location { get; init; }
        = Option<Geo>.None();
}
