using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.Common.Model.Telegram;

public record TelegramPort(TelegramUserId UserId, TelegramChatId ChatId);