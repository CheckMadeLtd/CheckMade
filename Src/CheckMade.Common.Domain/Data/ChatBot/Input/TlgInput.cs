using CheckMade.Common.Domain.Interfaces.Data.Core;
using CheckMade.Common.Utils.FpExtensions.Monads;

namespace CheckMade.Common.Domain.Data.ChatBot.Input;

public sealed record TlgInput(
    DateTimeOffset TlgDate,
    MessageId MessageId, 
    TlgAgent TlgAgent,
    InputType InputType,
    Option<IRoleInfo> OriginatorRole,
    Option<ILiveEventInfo> LiveEventContext,
    Option<ResultantWorkflowState> ResultantState,
    Option<Guid> EntityGuid,
    Option<string> CallbackQueryId,
    InputDetails Details);