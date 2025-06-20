using CheckMade.Core.Model.Common.CrossCutting;
using static CheckMade.Core.Model.Utils.Comparers.LiveEventInfoComparer;

namespace CheckMade.Core.Model.Common.LiveEvents;

public sealed record LiveEventInfo(
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
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
            LiveEventInfo liveEventInfo => Equals(liveEventInfo),
            LiveEvent liveEvent => Equals(liveEvent),
            null => false,
            _ => throw new InvalidOperationException("Every subtype should be explicitly handled")
        };
    }

    private bool Equals(LiveEvent other) =>
        AreEqual(this, other);

    public bool Equals(LiveEventInfo? other) =>
        other is not null &&
        AreEqual(this, other);

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, StartDate, EndDate, Status);
    }
}