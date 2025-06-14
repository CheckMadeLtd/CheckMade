using CheckMade.Common.DomainModel.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Core.Structs;
using CheckMade.Common.DomainModel.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;
using static CheckMade.Common.DomainModel.Utils.Comparers.UserInfoComparer;

namespace CheckMade.Common.DomainModel.Core.Actors.Concrete;

public sealed record User(
    MobileNumber Mobile,
    string FirstName,
    Option<string> MiddleName,
    string LastName,
    Option<EmailAddress> Email,
    LanguageCode Language,
    IReadOnlyCollection<IRoleInfo> HasRoles,
    Option<Vendor> CurrentEmployer,
    DbRecordStatus Status = DbRecordStatus.Active)
    : IUserInfo
{
    public User(
        IUserInfo userInfo,
        IReadOnlyCollection<IRoleInfo> roles,
        Option<Vendor> vendor) 
        : this(
            userInfo.Mobile,
            userInfo.FirstName,
            userInfo.MiddleName,
            userInfo.LastName,
            userInfo.Email,
            userInfo.Language,
            roles,
            vendor,
            userInfo.Status)
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
    
    private bool Equals(UserInfo other) =>
        AreEqual(this, other);

    public bool Equals(User? other) =>
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