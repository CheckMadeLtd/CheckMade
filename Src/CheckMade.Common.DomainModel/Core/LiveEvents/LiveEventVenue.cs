namespace CheckMade.Common.DomainModel.Core.LiveEvents;

public sealed record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);