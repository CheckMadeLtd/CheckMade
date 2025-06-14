using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.Common.DomainModel.Core.LiveEvents;

public interface ILiveEventInfo
{
    string Name { get; }
    DateTimeOffset StartDate { get; }
    DateTimeOffset EndDate { get; }
    DbRecordStatus Status { get; }
    
    bool Equals(ILiveEventInfo? other);
}