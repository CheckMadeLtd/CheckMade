using CheckMade.Common.LangExt;
using CheckMade.Common.Model.Enums;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto(
    UiString Text,
    Option<IEnumerable<ControlPrompts>> ControlPromptsSelection,
    Option<IEnumerable<DomainCategory>> DomainCategorySelection,
    Option<IEnumerable<string>> PredefinedChoices);
    