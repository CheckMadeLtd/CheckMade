using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Utils;

internal static class UserInfoComparer
{
    public static bool AreEqual(IUserInfo first, IUserInfo second)
    {
        return first.Mobile.ToString() == second.Mobile.ToString() && 
               first.FirstName == second.FirstName &&
               first.MiddleName.GetValueOrDefault() == second.MiddleName.GetValueOrDefault() &&
               first.LastName == second.LastName &&
               first.Email.GetValueOrDefault().ToString() == second.Email.GetValueOrDefault().ToString() &&
               first.Language == second.Language &&
               first.Status == second.Status;
    }
}