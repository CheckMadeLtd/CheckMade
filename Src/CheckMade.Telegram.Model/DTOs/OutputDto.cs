using CheckMade.Common.LangExt;
using CheckMade.Common.Model;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.TelegramUpdates;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto
{
    public OutputDestination Destination { get; }
    public Option<UiString> Text { get; }
    public Option<IEnumerable<DomainCategory>> DomainCategorySelection { get; }
    public Option<IEnumerable<ControlPrompts>> ControlPromptsSelection { get; }
    public Option<IEnumerable<string>> PredefinedChocies { get; }

    private OutputDto(
        OutputDestination destination,
        Option<UiString> text,
        Option<IEnumerable<DomainCategory>> domainCategories,
        Option<IEnumerable<ControlPrompts>> controlPrompts,
        Option<IEnumerable<string>> predefinedChocies)
    {
        Destination = destination;
        Text = text;
        DomainCategorySelection = domainCategories;
        ControlPromptsSelection = controlPrompts;
        PredefinedChocies = predefinedChocies;
    }

    public static OutputDto Create(UiString text) => 
        new(
            new OutputDestination(BotType.Operations, new Role("token", RoleType.SanitaryOps_Admin)),
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());
    
    public static OutputDto Create(OutputDestination destination, UiString text) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(OutputDestination destination, IEnumerable<DomainCategory> domainCategories) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(OutputDestination destination, IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(OutputDestination destination, IEnumerable<string> predefinedChocies) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.Some(predefinedChocies));

    public static OutputDto Create(
        OutputDestination destination, UiString text, IEnumerable<DomainCategory> domainCategories) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(
        OutputDestination destination, UiString text, IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(
        OutputDestination destination,
        UiString text,
        IEnumerable<DomainCategory> domainCategories,
        IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(
        OutputDestination destination,
        UiString text,
        IEnumerable<string> predefinedChoices) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.Some(predefinedChoices));

    public static OutputDto Create(
        OutputDestination destination,
        IEnumerable<DomainCategory> domainCategories,
        IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None());
}
