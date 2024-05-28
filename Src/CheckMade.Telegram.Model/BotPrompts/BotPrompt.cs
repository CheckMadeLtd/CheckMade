using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotPrompts;

public record BotPrompt(UiString Text, string Id, params BotType[] SupportedBotTypes);