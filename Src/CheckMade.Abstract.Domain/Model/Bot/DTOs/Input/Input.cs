using CheckMade.Abstract.Domain.Model.Bot.Categories;
using CheckMade.Abstract.Domain.Model.Common.Actors;
using CheckMade.Abstract.Domain.Model.Common.LiveEvents;
using General.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Model.Bot.DTOs.Input;

public sealed record Input(
    DateTimeOffset TimeStamp,
    MessageId MessageId, 
    Agent Agent,
    InputType InputType,
    Option<IRoleInfo> OriginatorRole,
    Option<ILiveEventInfo> LiveEventContext,
    Option<ResultantWorkflowState> ResultantState,
    Option<Guid> EntityGuid,
    Option<string> CallbackQueryId,
    InputDetails Details);