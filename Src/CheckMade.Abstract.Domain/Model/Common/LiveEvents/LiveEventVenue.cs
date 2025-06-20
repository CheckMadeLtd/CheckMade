using CheckMade.Abstract.Domain.Model.Common.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Common.LiveEvents;

public sealed record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);