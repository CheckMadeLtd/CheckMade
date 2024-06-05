using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Common.Model.Telegram;

public record TelegramOutputDestination(Role DestinationRole, BotType DestinationBotType);