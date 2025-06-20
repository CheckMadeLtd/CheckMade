using CheckMade.Core.Model.Common.CrossCutting;

namespace CheckMade.Core.Model.Common.Actors;

public sealed record Vendor(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);