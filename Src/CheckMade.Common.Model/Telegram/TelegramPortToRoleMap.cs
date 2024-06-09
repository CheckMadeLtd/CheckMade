namespace CheckMade.Common.Model.Telegram;

public record TelegramPortToRoleMap(
    Role Role,
    TelegramPort Port,
    DateTime ActivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);