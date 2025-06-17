namespace CheckMade.Abstract.Domain.Data.Core.LiveEvents;

public sealed record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);