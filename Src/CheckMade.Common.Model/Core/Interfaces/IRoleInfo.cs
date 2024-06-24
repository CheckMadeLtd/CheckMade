using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.Core.Interfaces;

public interface IRoleInfo
{
    string Token { get; }
    RoleType RoleType { get; }
    User User { get; }
    DbRecordStatus Status { get; }
}