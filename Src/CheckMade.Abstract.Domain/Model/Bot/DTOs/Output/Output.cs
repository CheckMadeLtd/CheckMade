using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Core.CrossCutting;
using CheckMade.Abstract.Domain.Model.Core.GIS;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs.Output;

public sealed record Output
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
