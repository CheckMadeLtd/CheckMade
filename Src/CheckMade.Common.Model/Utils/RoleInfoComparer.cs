using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Utils;

internal static class RoleInfoComparer
{
    public static bool AreEqual(IRoleInfo first, IRoleInfo second)
    {
        return first.Token == second.Token && 
               first.RoleType == second.RoleType &&
               first.Status == second.Status;
    }
}