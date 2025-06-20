using CheckMade.Abstract.Domain.Model.Core.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Core.Actors;

public sealed record Vendor(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);