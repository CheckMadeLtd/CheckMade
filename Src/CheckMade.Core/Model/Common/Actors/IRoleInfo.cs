using CheckMade.Core.Model.Common.CrossCutting;

namespace CheckMade.Core.Model.Common.Actors;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}