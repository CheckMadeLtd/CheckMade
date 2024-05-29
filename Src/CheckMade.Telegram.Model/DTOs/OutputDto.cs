using CheckMade.Common.LangExt;
using CheckMade.Telegram.Model.BotPrompts;

namespace CheckMade.Telegram.Model.DTOs;

public record OutputDto(
    UiString Text,
    Option<IEnumerable<ModelBotPrompt>> BotPrompts,
    Option<IEnumerable<string>> PredefinedChoices);
    