using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEvent(
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    LiveEventSeries Series,
    IEnumerable<Role> Roles,
    LiveEventVenue Venue,
    DbRecordStatus Status = DbRecordStatus.Active);