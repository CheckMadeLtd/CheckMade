using CheckMade.Common.DomainModel.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Core;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}