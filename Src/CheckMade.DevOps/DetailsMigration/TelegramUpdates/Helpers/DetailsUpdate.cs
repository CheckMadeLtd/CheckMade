using CheckMade.Common.Model.Telegram.Updates;

namespace CheckMade.DevOps.DetailsMigration.TelegramUpdates.Helpers;

internal record DetailsUpdate(
    TelegramUserId UserId, 
    DateTime TelegramDate, 
    string NewDetails);