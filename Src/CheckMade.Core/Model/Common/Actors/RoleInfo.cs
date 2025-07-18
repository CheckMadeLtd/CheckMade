using CheckMade.Core.Model.Common.CrossCutting;
using static CheckMade.Core.Model.Utils.Comparers.RoleInfoComparer;

namespace CheckMade.Core.Model.Common.Actors;

public sealed record RoleInfo(
        string Token,
        IRoleType RoleType,
        DbRecordStatus Status = DbRecordStatus.Active) 
    : IRoleInfo
{
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

    private bool Equals(Role other) =>
        AreEqual(this, other);

    public bool Equals(RoleInfo? other) =>
        other is not null &&
        AreEqual(this, other);
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Token, RoleType.GetType(), Status);
    }
}