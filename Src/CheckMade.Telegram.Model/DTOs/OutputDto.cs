using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.BotResponsePrompts;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto(
    UiString Text,
    Option<IEnumerable<BotResponsePrompt>> BotOperations,
    Option<IEnumerable<string>> PredefinedChoices);
    