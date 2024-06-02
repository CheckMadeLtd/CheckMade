using CheckMade.Common.Model.TelegramUpdates;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal record DetailsUpdate(
    TelegramUserId UserId, 
    DateTime TelegramDate, 
    string NewDetails);