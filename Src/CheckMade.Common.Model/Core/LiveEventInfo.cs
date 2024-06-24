using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEventInfo(
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        LiveEventVenue Venue,
        DbRecordStatus Status) 
    : ILiveEventInfo;