using CheckMade.Common.DomainModel.Data.Core;
using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.Data.ChatBot;

public sealed record TlgAgentRoleBind(
    Role Role,
    TlgAgent TlgAgent,
    DateTimeOffset ActivationDate,
    Option<DateTimeOffset> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);