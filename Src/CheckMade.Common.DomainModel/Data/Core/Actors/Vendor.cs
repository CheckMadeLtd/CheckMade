namespace CheckMade.Common.DomainModel.Data.Core.Actors;

public sealed record Vendor(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);