using CheckMade.Common.Model.TelegramUpdates;

namespace CheckMade.DevOps.DetailsMigration.TelegramUpdates.Helpers;

internal record DetailsUpdate(
    TelegramUserId UserId, 
    DateTime TelegramDate, 
    string NewDetails);