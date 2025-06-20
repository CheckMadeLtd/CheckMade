using CheckMade.Abstract.Domain.Model.Common.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Common.Actors;

public sealed record Vendor(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);