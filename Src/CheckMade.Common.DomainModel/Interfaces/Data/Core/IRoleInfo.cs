using CheckMade.Common.DomainModel.Data.Core;

namespace CheckMade.Common.DomainModel.Interfaces.Data.Core;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}