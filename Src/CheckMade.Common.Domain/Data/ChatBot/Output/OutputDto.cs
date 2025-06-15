using CheckMade.Common.Domain.Data.ChatBot.UserInteraction;
using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.GIS;
using CheckMade.Common.Utils.FpExtensions.Monads;
using CheckMade.Common.Utils.UiTranslation;

namespace CheckMade.Common.Domain.Data.ChatBot.Output;

public sealed record OutputDto
{
    public Option<LogicalPort> LogicalPort { get; init; } 
        = Option<LogicalPort>.None();
    
    public Option<UiString> Text { get; init; } 
        = Option<UiString>.None();
    
    public Option<IReadOnlyCollection<DomainTerm>> DomainTermSelection { get; init; } 
        = Option<IReadOnlyCollection<DomainTerm>>.None();
    
    public Option<ControlPrompts> ControlPromptsSelection { get; init; } 
        = Option<ControlPrompts>.None();
    
    public Option<IReadOnlyCollection<string>> PredefinedChoices { get; init; } 
        = Option<IReadOnlyCollection<string>>.None();
    
    public Option<IReadOnlyCollection<AttachmentDetails>> Attachments { get; init; }
        = Option<IReadOnlyCollection<AttachmentDetails>>.None();
    
    public Option<Geo> Location { get; init; }
        = Option<Geo>.None();
    
    public Option<MessageId> UpdateExistingOutputMessageId { get; init; }
        = Option<MessageId>.None();
    
    public Option<string> CallbackQueryId { get; init; }
        = Option<string>.None();

    public ActualSendOutParams? ActualSendOutParams { get; init; } = null;
}
