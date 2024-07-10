using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Actors.RoleSystem;

public interface IRoleInfo
{
    string Token { get; }
    RoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}