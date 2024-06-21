using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEventSeries(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);