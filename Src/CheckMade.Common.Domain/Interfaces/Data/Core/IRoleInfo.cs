using CheckMade.Common.Domain.Data.Core;

namespace CheckMade.Common.Domain.Interfaces.Data.Core;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}