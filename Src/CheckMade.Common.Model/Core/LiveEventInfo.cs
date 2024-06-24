using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEventInfo(
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        DbRecordStatus Status)
    : ILiveEventInfo
{
    public LiveEventInfo(LiveEvent liveEvent) 
        : this(liveEvent.Name, liveEvent.StartDate, liveEvent.EndDate, liveEvent.Status)
    {
    }
}