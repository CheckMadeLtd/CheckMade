using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal sealed record OldFormatDetails(
    HistoricInputIdentifier Identifier, 
    JObject OldFormatDetailsJson);