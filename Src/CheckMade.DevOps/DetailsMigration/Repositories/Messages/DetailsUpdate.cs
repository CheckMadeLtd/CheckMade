namespace CheckMade.DevOps.DetailsMigration.Repositories.Messages;

internal record DetailsUpdate(
    long UserId, 
    DateTime TelegramDate, 
    string NewDetails);