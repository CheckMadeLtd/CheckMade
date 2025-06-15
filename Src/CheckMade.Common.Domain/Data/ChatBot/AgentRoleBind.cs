using CheckMade.Common.Domain.Data.Core;
using CheckMade.Common.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.ChatBot;

public sealed record AgentRoleBind(
    Role Role,
    TlgAgent TlgAgent,
    DateTimeOffset ActivationDate,
    Option<DateTimeOffset> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);