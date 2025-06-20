using CheckMade.Abstract.Domain.Model.Core.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Core.LiveEvents;

public sealed record LiveEventSeries(
    string Name,
    IReadOnlyCollection<LiveEvent> LiveEvents,
    DbRecordStatus Status = DbRecordStatus.Active);