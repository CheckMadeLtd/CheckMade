using CheckMade.Core.Model.Common.LiveEvents;

namespace CheckMade.Core.Model.Utils.Comparers;

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