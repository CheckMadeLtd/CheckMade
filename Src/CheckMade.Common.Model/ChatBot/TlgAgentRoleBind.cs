using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.ChatBot;

public record TlgAgentRoleBind(
    Role Role,
    TlgAgent TlgAgent,
    DateTime ActivationDate,
    Option<DateTime> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);