using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Common.Model.ChatBot;

public sealed record TlgAgentRoleBind(
    Role Role,
    TlgAgent TlgAgent,
    DateTimeOffset ActivationDate,
    Option<DateTimeOffset> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);