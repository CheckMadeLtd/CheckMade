using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal sealed record OldFormatDetails(
    int Id, 
    JObject OldFormatDetailsJson);