using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Data.Bot;

public sealed record AgentRoleBind(
    Role Role,
    Agent Agent,
    DateTimeOffset ActivationDate,
    Option<DateTimeOffset> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);