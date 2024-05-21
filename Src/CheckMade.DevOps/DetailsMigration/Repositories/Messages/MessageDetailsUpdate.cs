namespace CheckMade.DevOps.DetailsMigration.Repositories.Messages;

internal record MessageDetailsUpdate(
    long UserId, 
    DateTime TelegramDate, 
    string NewDetails);