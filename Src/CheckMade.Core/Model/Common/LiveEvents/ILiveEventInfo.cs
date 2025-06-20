using CheckMade.Core.Model.Common.CrossCutting;

namespace CheckMade.Core.Model.Common.LiveEvents;

public interface ILiveEventInfo
{
    string Name { get; }
    DateTimeOffset StartDate { get; }
    DateTimeOffset EndDate { get; }
    DbRecordStatus Status { get; }
    
    bool Equals(ILiveEventInfo? other);
}