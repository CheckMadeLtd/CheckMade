using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.BotPrompts;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto(
    UiString Text,
    Option<IEnumerable<EBotPrompts>> BotPromptSelection,
    Option<IEnumerable<string>> PredefinedChoices);
    