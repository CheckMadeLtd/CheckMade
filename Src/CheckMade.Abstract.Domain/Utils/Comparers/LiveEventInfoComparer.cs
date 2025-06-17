using CheckMade.Abstract.Domain.Interfaces.Data.Core;

namespace CheckMade.Abstract.Domain.Utils.Comparers;

internal static class LiveEventInfoComparer
{
    public static bool AreEqual(ILiveEventInfo first, ILiveEventInfo second)
    {
        return first.Name.Equals(second.Name) && 
               first.StartDate.Equals(second.StartDate) &&
               first.EndDate.Equals(second.EndDate) &&
               first.Status.Equals(second.Status);
    }
}