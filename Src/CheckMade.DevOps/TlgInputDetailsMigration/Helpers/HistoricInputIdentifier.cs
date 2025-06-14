using CheckMade.Common.DomainModel.ChatBot.Input;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal record HistoricInputIdentifier(TlgUserId HistoricUserId, DateTimeOffset HistoricTlgDate);