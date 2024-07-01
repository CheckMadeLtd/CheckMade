using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.Utils;

internal static class UserInfoComparer
{
    public static bool AreEqual(IUserInfo first, IUserInfo second)
    {
        return first.Mobile.ToString().Equals(second.Mobile.ToString()) && 
               first.FirstName.Equals(second.FirstName) &&
               Equals(first.MiddleName.GetValueOrDefault(), second.MiddleName.GetValueOrDefault()) &&
               first.LastName.Equals(second.LastName) &&
               Equals(first.Email.GetValueOrDefault().ToString(), second.Email.GetValueOrDefault().ToString()) &&
               first.Language.Equals(second.Language) &&
               first.Status.Equals(second.Status);
    }
}