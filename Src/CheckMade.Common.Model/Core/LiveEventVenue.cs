using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);