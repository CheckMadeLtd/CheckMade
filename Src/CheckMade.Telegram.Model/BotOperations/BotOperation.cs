using CheckMade.Common.LangExt;

namespace CheckMade.Telegram.Model.BotOperations;

public record BotOperation(UiString OpText, string OpId, params BotType[] SupportedBotTypes);