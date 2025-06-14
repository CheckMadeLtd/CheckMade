using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.Common.DomainModel.Core.LiveEvents.Concrete;

public sealed record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);