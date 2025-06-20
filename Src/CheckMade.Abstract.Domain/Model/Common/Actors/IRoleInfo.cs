using CheckMade.Abstract.Domain.Model.Common.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Common.Actors;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}