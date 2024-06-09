using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto
{
    public Option<LogicalPort> LogicalPort { get; init; } 
        = Option<LogicalPort>.None();
    
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
