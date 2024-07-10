using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Utils.Comparers;

internal static class RoleInfoComparer
{
    public static bool AreEqual(IRoleInfo first, IRoleInfo second)
    {
        return first.Token.Equals(second.Token) && 
               first.RoleType.Equals(second.RoleType) &&
               first.Status.Equals(second.Status);
    }
}