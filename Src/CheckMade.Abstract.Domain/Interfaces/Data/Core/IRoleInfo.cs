using CheckMade.Abstract.Domain.Data.Core;

namespace CheckMade.Abstract.Domain.Interfaces.Data.Core;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}