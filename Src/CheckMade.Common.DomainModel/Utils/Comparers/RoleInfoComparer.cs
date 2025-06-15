using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Common.DomainModel.Utils.Comparers;

internal static class RoleInfoComparer
{
    public static bool AreEqual(IRoleInfo first, IRoleInfo second)
    {
        return first.Token.Equals(second.Token) && 
               first.RoleType.GetType() == second.RoleType.GetType() &&
               first.Status.Equals(second.Status);
    }
}