using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Abstract.Domain.Data.ChatBot.Input;

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