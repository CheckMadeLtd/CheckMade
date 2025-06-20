using CheckMade.Core.Model.Common.Actors;
using CheckMade.Core.Model.Common.CrossCutting;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Core.Model.Bot.DTOs;

public sealed record AgentRoleBind(
    Role Role,
    Agent Agent,
    DateTimeOffset ActivationDate,
    Option<DateTimeOffset> DeactivationDate,
    DbRecordStatus Status = DbRecordStatus.Active);