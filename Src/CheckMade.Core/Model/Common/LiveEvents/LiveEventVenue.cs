using CheckMade.Core.Model.Common.CrossCutting;

namespace CheckMade.Core.Model.Common.LiveEvents;

public sealed record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);