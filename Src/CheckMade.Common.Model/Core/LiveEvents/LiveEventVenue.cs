using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.LiveEvents;

public record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);