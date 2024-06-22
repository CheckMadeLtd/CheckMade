using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEventSeries(
    string Name,
    IEnumerable<LiveEvent> LiveEvents,
    DbRecordStatus Status = DbRecordStatus.Active);