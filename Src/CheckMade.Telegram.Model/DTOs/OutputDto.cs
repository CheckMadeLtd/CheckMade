using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Enums;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto
{
    public Option<UiString> Text { get; }
    public Option<IEnumerable<DomainCategory>> DomainCategorySelection { get; }
    public Option<IEnumerable<ControlPrompts>> ControlPromptsSelection { get; }
    public Option<IEnumerable<string>> PredefinedChocies { get; }

    private OutputDto(
        Option<UiString> text,
        Option<IEnumerable<DomainCategory>> domainCategories,
        Option<IEnumerable<ControlPrompts>> controlPrompts,
        Option<IEnumerable<string>> predefinedChocies)
    {
        Text = text;
        DomainCategorySelection = domainCategories;
        ControlPromptsSelection = controlPrompts;
        PredefinedChocies = predefinedChocies;
    }

    public static OutputDto CreateEmpty() => new OutputDto(
        Option<UiString>.None(),
        Option<IEnumerable<DomainCategory>>.None(),
        Option<IEnumerable<ControlPrompts>>.None(),
        Option<IEnumerable<string>>.None());

    public static OutputDto Create(UiString text) => new OutputDto(
        text,
        Option<IEnumerable<DomainCategory>>.None(),
        Option<IEnumerable<ControlPrompts>>.None(),
        Option<IEnumerable<string>>.None());

    public static OutputDto Create(IEnumerable<DomainCategory> domainCategories) => new OutputDto(
        Option<UiString>.None(),
        Option<IEnumerable<DomainCategory>>.Some(domainCategories),
        Option<IEnumerable<ControlPrompts>>.None(),
        Option<IEnumerable<string>>.None());

    public static OutputDto Create(IEnumerable<ControlPrompts> controlPrompts) => new OutputDto(
        Option<UiString>.None(),
        Option<IEnumerable<DomainCategory>>.None(),
        Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
        Option<IEnumerable<string>>.None());

    public static OutputDto Create(IEnumerable<string> predefinedChocies) => new OutputDto(
        Option<UiString>.None(),
        Option<IEnumerable<DomainCategory>>.None(),
        Option<IEnumerable<ControlPrompts>>.None(),
        Option<IEnumerable<string>>.Some(predefinedChocies));

    public static OutputDto Create(UiString text, IEnumerable<DomainCategory> domainCategories) => new OutputDto(
        text,
        Option<IEnumerable<DomainCategory>>.Some(domainCategories),
        Option<IEnumerable<ControlPrompts>>.None(),
        Option<IEnumerable<string>>.None());

    public static OutputDto Create(UiString text, IEnumerable<ControlPrompts> controlPrompts) => new OutputDto(
        text,
        Option<IEnumerable<DomainCategory>>.None(),
        Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
        Option<IEnumerable<string>>.None());

    public static OutputDto Create(
        UiString text,
        IEnumerable<DomainCategory> domainCategories,
        IEnumerable<ControlPrompts> controlPrompts) => new OutputDto(
        text,
        Option<IEnumerable<DomainCategory>>.Some(domainCategories),
        Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
        Option<IEnumerable<string>>.None());

    public static OutputDto Create(UiString text, IEnumerable<string> predefinedChoices) => new OutputDto(
        text,
        Option<IEnumerable<DomainCategory>>.None(),
        Option<IEnumerable<ControlPrompts>>.None(),
        Option<IEnumerable<string>>.Some(predefinedChoices));

    public static OutputDto Create(
        IEnumerable<DomainCategory> domainCategories,
        IEnumerable<ControlPrompts> controlPrompts) => new OutputDto(
        Option<UiString>.None(),
        Option<IEnumerable<DomainCategory>>.Some(domainCategories),
        Option<IEnumerable<ControlPrompts>>.Some(controlPrompts),
        Option<IEnumerable<string>>.None());
}
