using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal abstract record DetailsUpdate(
    TlgUserId UserId, 
    DateTimeOffset TlgDate, 
    string NewDetails);