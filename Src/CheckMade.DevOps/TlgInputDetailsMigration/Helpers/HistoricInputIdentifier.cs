using CheckMade.Common.Model.ChatBot.Input;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal record HistoricInputIdentifier(TlgUserId HistoricUserId, DateTimeOffset HistoricTlgDate);