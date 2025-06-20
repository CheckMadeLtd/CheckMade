using CheckMade.Abstract.Domain.Model.Common.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Common.LiveEvents;

public sealed record LiveEventSeries(
    string Name,
    IReadOnlyCollection<LiveEvent> LiveEvents,
    DbRecordStatus Status = DbRecordStatus.Active);