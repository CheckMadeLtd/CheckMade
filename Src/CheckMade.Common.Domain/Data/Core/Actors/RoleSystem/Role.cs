using CheckMade.Common.Domain.Interfaces.Data.Core;
using static CheckMade.Common.Domain.Utils.Comparers.RoleInfoComparer;
    
namespace CheckMade.Common.Domain.Data.Core.Actors.RoleSystem;

public sealed record Role(
    string Token,
    IRoleType RoleType,
    IUserInfo ByUser,
    ILiveEventInfo AtLiveEvent,
    IReadOnlyCollection<ISphereOfAction> AssignedToSpheres,
    DbRecordStatus Status = DbRecordStatus.Active)
    : IRoleInfo
{
    public Role(IRoleInfo roleInfo, IUserInfo userInfo, ILiveEventInfo liveEventInfo, 
        IReadOnlyCollection<ISphereOfAction> assignedSpheres)
        : this(
            roleInfo.Token,
            roleInfo.RoleType,
            userInfo,
            liveEventInfo,
            assignedSpheres,
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
        other is not null &&
        AreEqual(this, other);
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Token, RoleType.GetType(), Status);
    }
}