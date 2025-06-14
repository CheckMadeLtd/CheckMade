using CheckMade.Common.DomainModel.Interfaces.Core;
using CheckMade.Common.LangExt.FpExtensions.Monads;

namespace CheckMade.Common.DomainModel.ChatBot.Input;

public sealed record TlgInput(
    DateTimeOffset TlgDate,
    TlgMessageId TlgMessageId, 
    TlgAgent TlgAgent,
    TlgInputType InputType,
    Option<IRoleInfo> OriginatorRole,
    Option<ILiveEventInfo> LiveEventContext,
    Option<ResultantWorkflowState> ResultantState,
    Option<Guid> EntityGuid,
    Option<string> CallbackQueryId,
    TlgInputDetails Details);