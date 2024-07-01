using CheckMade.Common.Model.Core.Interfaces;
using CheckMade.Common.Model.Utils;
using static CheckMade.Common.Model.Utils.RoleInfoComparer;
    
namespace CheckMade.Common.Model.Core;

public sealed record Role(
        string Token,
        RoleType RoleType,
        IUserInfo ByUser,
        ILiveEventInfo AtLiveEvent,
        DbRecordStatus Status = DbRecordStatus.Active)
    : IRoleInfo
{
    public Role(IRoleInfo roleInfo, IUserInfo userInfo, ILiveEventInfo liveEventInfo)
        : this(
            roleInfo.Token,
            roleInfo.RoleType,
            userInfo,
            liveEventInfo,
            roleInfo.Status)
    {
    }

    public bool Equals(IRoleInfo? other)
    {
        return other switch
        {
            RoleInfo roleInfo => Equals(roleInfo),
            Role role => Equals(role), 
            null => false,
            _ => throw new InvalidOperationException("Every subtype should be explicitly handled")
        };
    }

    private bool Equals(RoleInfo other) =>
        AreEqual(this, other);

    public bool Equals(Role? other) =>
        AreEqual(this, other!);
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Token, RoleType, Status);
    }
}