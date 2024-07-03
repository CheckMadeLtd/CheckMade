using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.LiveEvents;

public record LiveEventSeries(
    string Name,
    IReadOnlyCollection<LiveEvent> LiveEvents,
    DbRecordStatus Status = DbRecordStatus.Active);