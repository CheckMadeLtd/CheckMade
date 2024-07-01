using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Utils.LiveEventInfoComparer;

namespace CheckMade.Common.Model.Core;

public sealed record LiveEvent : ILiveEventInfo
{
    public LiveEvent(
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        IReadOnlyCollection<IRoleInfo> WithRoles,
        LiveEventVenue AtVenue,
        DbRecordStatus Status = DbRecordStatus.Active)
    {
        if (EndDate < StartDate)
            throw new ArgumentException($"{nameof(EndDate)} must be after {nameof(StartDate)} for any LiveEvent!");
        
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.WithRoles = WithRoles;
        this.AtVenue = AtVenue;
        this.Status = Status;
    }
    
    public LiveEvent(ILiveEventInfo liveEventInfo,IReadOnlyCollection<IRoleInfo> roles, LiveEventVenue venue)
    : this(
        liveEventInfo.Name,
        liveEventInfo.StartDate,
        liveEventInfo.EndDate,
        roles,
        venue,
        liveEventInfo.Status)
    {}

    public string Name { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IReadOnlyCollection<IRoleInfo> WithRoles { get; init; }
    public LiveEventVenue AtVenue { get; init; }
    public DbRecordStatus Status { get; init; }
    
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

    private bool Equals(LiveEventInfo other) =>
        AreEqual(this, other);

    public bool Equals(LiveEvent? other) =>
        other is not null &&
        AreEqual(this, other);
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, StartDate, EndDate, Status);
    }
}
