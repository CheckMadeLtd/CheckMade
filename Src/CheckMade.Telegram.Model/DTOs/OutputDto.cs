using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Enums;
using CheckMade.Common.Model.Telegram;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto
{
    public Option<TelegramOutputDestination> ExplicitDestination { get; }
    public Option<UiString> Text { get; }
    public Option<IEnumerable<DomainCategory>> DomainCategorySelection { get; }
    public Option<IEnumerable<ControlPrompts>> ControlPromptsSelection { get; }
    public Option<IEnumerable<string>> PredefinedChoices { get; }

    private OutputDto(
        Option<TelegramOutputDestination> explicitDestination,
        Option<UiString> text,
        Option<IEnumerable<DomainCategory>> domainCategories,
        Option<IEnumerable<ControlPrompts>> controlPrompts,
        Option<IEnumerable<string>> predefinedChoices)
    {
        ExplicitDestination = explicitDestination;
        Text = text;
        DomainCategorySelection = domainCategories;
        ControlPromptsSelection = controlPrompts;
        PredefinedChoices = predefinedChoices;
    }

    public static OutputDto Create(UiString text) => 
        new(
            Option<TelegramOutputDestination>.None(), 
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());
    
    public static OutputDto Create(TelegramOutputDestination destination, UiString text) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(TelegramOutputDestination destination, IEnumerable<DomainCategory> domainCategories) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(TelegramOutputDestination destination, IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(TelegramOutputDestination destination, IEnumerable<string> predefinedChocies) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.Some(predefinedChocies));

    public static OutputDto Create(
        TelegramOutputDestination destination, UiString text, IEnumerable<DomainCategory> domainCategories) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination, UiString text, IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None());

    public static OutputDto Create(
        TelegramOutputDestination destination,
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
        TelegramOutputDestination destination,
        UiString text,
        IEnumerable<string> predefinedChoices) => 
        new(
            destination,
            text,
            Option<IEnumerable<DomainCategory>>.None(),
            Option<IEnumerable<ControlPrompts>>.None(),
            Option<IEnumerable<string>>.Some(predefinedChoices));

    public static OutputDto Create(
        TelegramOutputDestination destination,
        IEnumerable<DomainCategory> domainCategories,
        IEnumerable<ControlPrompts> controlPrompts) => 
        new(
            destination,
            Option<UiString>.None(),
            Option<IEnumerable<DomainCategory>>.Some(domainCategories),
            Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
            Option<IEnumerable<string>>.None());
}
