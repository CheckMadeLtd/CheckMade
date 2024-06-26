using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEvent : ILiveEventInfo
{
    public LiveEvent(
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        IEnumerable<IRoleInfo> Roles,
        LiveEventVenue Venue,
        DbRecordStatus Status = DbRecordStatus.Active)
    {
        if (EndDate < StartDate)
            throw new ArgumentException($"{nameof(EndDate)} must be after {nameof(StartDate)} for any LiveEvent!");
        
        this.Name = Name;
        this.StartDate = StartDate;
        this.EndDate = EndDate;
        this.Roles = Roles;
        this.Venue = Venue;
        this.Status = Status;
    }
    
    public LiveEvent(ILiveEventInfo liveEventInfo,IEnumerable<IRoleInfo> roles, LiveEventVenue venue)
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
    public IEnumerable<IRoleInfo> Roles { get; init; }
    public LiveEventVenue Venue { get; init; }
    public DbRecordStatus Status { get; init; }
}