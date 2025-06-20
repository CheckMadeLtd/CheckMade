using CheckMade.Abstract.Domain.Model.Core.CrossCutting;

namespace CheckMade.Abstract.Domain.Model.Core.Actors;

public interface IRoleInfo
{
    string Token { get; }
    IRoleType RoleType { get; }
    DbRecordStatus Status { get; }

    bool Equals(IRoleInfo? other);
}