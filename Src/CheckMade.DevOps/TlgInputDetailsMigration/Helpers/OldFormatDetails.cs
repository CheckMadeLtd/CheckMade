using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.TlgInputDetailsMigration.Helpers;

internal sealed record OldFormatDetails(
    HistoricInputIdentifier Identifier, 
    JObject OldFormatDetailsJson);