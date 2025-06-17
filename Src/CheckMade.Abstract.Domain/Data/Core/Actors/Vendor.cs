namespace CheckMade.Abstract.Domain.Data.Core.Actors;

public sealed record Vendor(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);