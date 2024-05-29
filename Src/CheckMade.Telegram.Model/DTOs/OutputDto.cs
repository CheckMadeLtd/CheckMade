using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.ControlPrompt;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto(
    UiString Text,
    Option<IEnumerable<ControlPrompts>> ControlPromptsSelection,
    Option<IEnumerable<string>> PredefinedChoices);
    