namespace CheckMade.Common.Model.Telegram;

public record TelegramUserChatDestinationToRoleMap(
    Role Role,
    TelegramUserChatDestination UserChatDestination,
    DbRecordStatus Status = DbRecordStatus.Active);