using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Interfaces;

public interface ILiveEventInfo
{
    string Name { get; }
    DateTime StartDate { get; }
    DateTime EndDate { get; }
    DbRecordStatus Status { get; }
}