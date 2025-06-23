using CheckMade.Core.Model.Bot.DTOs.Inputs;

namespace CheckMade.DevOps.InputDetailsMigration.Helpers;

internal sealed record NewFormatDetails(
    HistoricInputIdentifier Identifier, 
    string NewDetails);