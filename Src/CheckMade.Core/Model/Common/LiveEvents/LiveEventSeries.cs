using CheckMade.Core.Model.Common.CrossCutting;

namespace CheckMade.Core.Model.Common.LiveEvents;

public sealed record LiveEventSeries(
    string Name,
    IReadOnlyCollection<LiveEvent> LiveEvents,
    DbRecordStatus Status = DbRecordStatus.Active);