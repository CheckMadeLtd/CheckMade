namespace CheckMade.Common.DomainModel.Data.Core.LiveEvents;

public sealed record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);