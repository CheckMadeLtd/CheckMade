using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotOperations;

public record BotResponsePrompt(UiString Text, string Id, params BotType[] SupportedBotTypes);