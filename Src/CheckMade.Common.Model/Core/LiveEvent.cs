using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core;

public record LiveEvent : LiveEventStub
{
    public LiveEvent(string Name,
        DateTime StartDate,
        DateTime EndDate,
        IEnumerable<Role> Roles,
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

    public string Name { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IEnumerable<Role> Roles { get; init; }
    public LiveEventVenue Venue { get; init; }
    public DbRecordStatus Status { get; init; }
}