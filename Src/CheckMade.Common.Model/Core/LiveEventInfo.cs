using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Utils.LiveEventInfoComparer;

namespace CheckMade.Common.Model.Core;

public record LiveEventInfo(
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        DbRecordStatus Status = DbRecordStatus.Active)
    : ILiveEventInfo
{
    public LiveEventInfo(LiveEvent liveEvent) 
        : this(
            liveEvent.Name,
            liveEvent.StartDate,
            liveEvent.EndDate,
            liveEvent.Status)
    {
    }
    
    public bool Equals(ILiveEventInfo? other)
    {
        return other switch
        {
            LiveEventInfo liveEventInfo => this == liveEventInfo,
            LiveEvent liveEvent => Equals(liveEvent),
            null => false,
            _ => throw new InvalidOperationException("Every subtype should be explicitly handled")
        };
    }
    
    protected virtual bool Equals(LiveEvent other)
    {
        return AreEqual(this, other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, StartDate, EndDate, Status);
    }
}