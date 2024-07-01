using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Utils;

internal static class LiveEventInfoComparer
{
    public static bool AreEqual(ILiveEventInfo first, ILiveEventInfo second)
    {
        return first.Name == second.Name && 
               first.StartDate == second.StartDate &&
               first.EndDate == second.EndDate &&
               first.Status == second.Status;
    }
}