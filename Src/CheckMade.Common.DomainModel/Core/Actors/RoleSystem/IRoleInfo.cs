using CheckMade.Common.DomainModel.Utils;

namespace CheckMade.Common.DomainModel.Core.Actors.RoleSystem;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}