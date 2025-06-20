using CheckMade.Abstract.Domain.Model.Common.Actors;
using CheckMade.Abstract.Domain.Model.Common.CrossCutting;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs;

public sealed record AgentRoleBind(
    Role Role,
    Agent Agent,
    DateTimeOffset ActivationDate,
    Option<DateTimeOffset> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);