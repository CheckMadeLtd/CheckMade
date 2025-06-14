using CheckMade.Common.DomainModel.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Utils;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.ChatBot;

public sealed record TlgAgentRoleBind(
    Role Role,
    TlgAgent TlgAgent,
    DateTimeOffset ActivationDate,
    Option<DateTimeOffset> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);