using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.ChatBot.Output;

public record OutputDto
{
    public Option<LogicalPort> LogicalPort { get; init; } 
        = Option<LogicalPort>.None();
    
    public Option<UiString> Text { get; init; } 
        = Option<UiString>.None();
    
    // ToDo: Rename it and here only the collection of OneOf<> !!!  Then the OutputToReplyMarkupConverter shall handle the resolution into CallbackId, UiString!
    public Option<IDictionary<CallbackId, UiString>> DomainTermSelection { get; init; } 
        = Option<IDictionary<CallbackId, UiString>>.None();
    
    public Option<ControlPrompts> ControlPromptsSelection { get; init; } 
        = Option<ControlPrompts>.None();
    
    public Option<IEnumerable<string>> PredefinedChoices { get; init; } 
        = Option<IEnumerable<string>>.None();
    
    public Option<IEnumerable<OutputAttachmentDetails>> Attachments { get; init; }
        = Option<IEnumerable<OutputAttachmentDetails>>.None();
    
    public Option<Geo> Location { get; init; }
        = Option<Geo>.None();
}
