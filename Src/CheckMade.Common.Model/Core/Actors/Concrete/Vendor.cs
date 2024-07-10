using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Actors.Concrete;

public record Vendor(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);