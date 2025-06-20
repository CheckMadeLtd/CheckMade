using CheckMade.Abstract.Domain.Model.Common.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Common.LiveEvents;

public interface ILiveEventInfo
{
    string Name { get; }
    DateTimeOffset StartDate { get; }
    DateTimeOffset EndDate { get; }
    DbRecordStatus Status { get; }
    
    bool Equals(ILiveEventInfo? other);
}