using CheckMade.Abstract.Domain.Data.Core;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface ILiveEventInfo
{
    string Name { get; }
    DateTimeOffset StartDate { get; }
    DateTimeOffset EndDate { get; }
    DbRecordStatus Status { get; }
    
    bool Equals(ILiveEventInfo? other);
}