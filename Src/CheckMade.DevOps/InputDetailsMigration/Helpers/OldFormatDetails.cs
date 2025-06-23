using CheckMade.Core.Model.Bot.DTOs.Inputs;
using Newtonsoft.Json.Linq;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal sealed record OldFormatDetails(
    HistoricInputIdentifier Identifier, 
    JObject OldFormatDetailsJson);