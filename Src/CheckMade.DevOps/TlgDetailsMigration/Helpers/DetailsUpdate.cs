using CheckMade.Common.Model.Telegram.Input;

namespace CheckMade.DevOps.TlgDetailsMigration.Helpers;

internal record DetailsUpdate(
    TlgUserId UserId, 
    DateTime TelegramDate, 
    string NewDetails);