using CheckMade.Common.DomainModel.Interfaces.Data.Core;
using static CheckMade.Common.DomainModel.Utils.Comparers.LiveEventInfoComparer;

namespace CheckMade.Common.DomainModel.Data.Core.LiveEvents;

public sealed record LiveEvent : ILiveEventInfo
{
    public LiveEvent(
        string Name,
        DateTimeOffset StartDate,
        DateTimeOffset EndDate,
        IReadOnlyCollection<IRoleInfo> WithRoles,
        LiveEventVenue AtVenue,
        IReadOnlyCollection<ISphereOfAction> DivIntoSpheres,
        DbRecordStatus Status = DbRecordStatus.Active)
    {
        if (EndDate < StartDate)
            throw new ArgumentException($"{nameof(EndDate)} must be after {nameof(StartDate)} for any LiveEvent!");
        
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.WithRoles = WithRoles;
        this.AtVenue = AtVenue;
        this.DivIntoSpheres = DivIntoSpheres;
        this.Status = Status;
    }
    
    public LiveEvent(
        ILiveEventInfo liveEventInfo,
        IReadOnlyCollection<IRoleInfo> roles,
        LiveEventVenue venue,
        IReadOnlyCollection<ISphereOfAction> spheres)
        : this(
            liveEventInfo.Name,
            liveEventInfo.StartDate,
            liveEventInfo.EndDate,
            roles,
            venue,
            spheres,
            liveEventInfo.Status)
    {
    }

    public string Name { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public IReadOnlyCollection<IRoleInfo> WithRoles { get; init; }
    public LiveEventVenue AtVenue { get; init; }
    public IReadOnlyCollection<ISphereOfAction> DivIntoSpheres { get; init; }
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
