namespace CheckMade.Common.DomainModel.Core.LiveEvents;

public sealed record LiveEventSeries(
    string Name,
    IReadOnlyCollection<LiveEvent> LiveEvents,
    DbRecordStatus Status = DbRecordStatus.Active);