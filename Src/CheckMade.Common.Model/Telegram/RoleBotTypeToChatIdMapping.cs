using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Common.Model.Telegram;

public record RoleBotTypeToChatIdMapping(Role Role, BotType BotType, TelegramChatId ChatId);