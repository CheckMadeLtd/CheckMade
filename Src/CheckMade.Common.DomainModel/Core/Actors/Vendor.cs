using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.Common.DomainModel.Core.Actors;

public sealed record Vendor(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);