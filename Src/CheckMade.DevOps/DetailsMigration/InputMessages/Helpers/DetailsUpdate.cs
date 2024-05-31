using CheckMade.Telegram.Model;

namespace CheckMade.DevOps.DetailsMigration.InputMessages.Helpers;

internal record DetailsUpdate(
    UserId UserId, 
    DateTime TelegramDate, 
    string NewDetails);