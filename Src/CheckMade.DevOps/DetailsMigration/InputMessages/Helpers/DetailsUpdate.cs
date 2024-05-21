namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal record DetailsUpdate(
    long UserId, 
    DateTime TelegramDate, 
    string NewDetails);