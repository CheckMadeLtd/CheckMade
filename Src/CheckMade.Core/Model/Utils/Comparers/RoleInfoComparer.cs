using CheckMade.Core.Model.Common.Actors;

namespace CheckMade.Core.Model.Utils.Comparers;

internal static class RoleInfoComparer
{
    public static bool AreEqual(IRoleInfo first, IRoleInfo second)
    {
        return first.Token.Equals(second.Token) && 
               first.RoleType.GetType() == second.RoleType.GetType() &&
               first.Status.Equals(second.Status);
    }
}