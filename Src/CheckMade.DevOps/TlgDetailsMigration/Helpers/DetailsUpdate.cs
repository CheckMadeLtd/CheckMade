using CheckMade.Common.Model.Tlg.Updates;

namespace CheckMade.DevOps.TlgDetailsMigration.Helpers;

internal record DetailsUpdate(
    TlgUserId UserId, 
    DateTime TelegramDate, 
    string NewDetails);