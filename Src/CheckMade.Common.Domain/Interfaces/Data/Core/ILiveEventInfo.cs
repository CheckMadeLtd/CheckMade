using CheckMade.Common.Domain.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface ILiveEventInfo
{
    string Name { get; }
    DateTimeOffset StartDate { get; }
    DateTimeOffset EndDate { get; }
    DbRecordStatus Status { get; }
    
    bool Equals(ILiveEventInfo? other);
}