namespace CheckMade.Common.Model.Telegram;

public record TelegramUserChatDestinationToRoleMap(
    Role Role,
    TelegramUserChatDestination UserChatDestination,
    DateTime ActivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);