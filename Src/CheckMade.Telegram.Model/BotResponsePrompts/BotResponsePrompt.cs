using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotResponsePrompts;

public record BotResponsePrompt(UiString Text, string Id, params BotType[] SupportedBotTypes);