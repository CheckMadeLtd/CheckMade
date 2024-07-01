using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Core.Structs;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Utils.UserInfoComparer;

namespace CheckMade.Common.Model.Core;

public sealed record UserInfo(
    MobileNumber Mobile,
    string FirstName,
    Option<string> MiddleName,
    string LastName,
    Option<EmailAddress> Email,
    LanguageCode Language,
    DbRecordStatus Status = DbRecordStatus.Active)
    : IUserInfo
{
    public UserInfo(User user)
        : this(
            user.Mobile,
            user.FirstName,
            user.MiddleName,
            user.LastName,
            user.Email,
            user.Language,
            user.Status
        )
    {
    }
    
    public bool Equals(IUserInfo? other)
    {
        return other switch
        {
            UserInfo userInfo => Equals(userInfo),
            User user => Equals(user),
            null => false,
            _ => throw new InvalidOperationException("Every subtype should be explicitly handled")
        };
    }
    
    private bool Equals(User other) =>
        AreEqual(this, other);

    public bool Equals(UserInfo? other) =>
        other is not null &&
        AreEqual(this, other);
    
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Mobile.ToString(),
            FirstName,
            MiddleName.GetValueOrDefault(),
            LastName,
            Email.GetValueOrDefault().ToString(),
            Language,
            Status);
    }
}