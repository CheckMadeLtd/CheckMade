using CheckMade.Abstract.Domain.Model.Core.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Core.LiveEvents;

public sealed record LiveEventVenue(
    string Name,
    DbRecordStatus Status = DbRecordStatus.Active);