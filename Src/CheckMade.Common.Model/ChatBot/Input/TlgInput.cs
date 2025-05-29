using CheckMade.Common.LangExt.FpExtensions.Monads;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.LiveEvents;

namespace CheckMade.Common.Model.ChatBot.Input;

public sealed record TlgInput(
    DateTimeOffset TlgDate,
    TlgMessageId TlgMessageId, 
    TlgAgent TlgAgent,
    TlgInputType InputType,
    Option<IRoleInfo> OriginatorRole,
    Option<ILiveEventInfo> LiveEventContext,
    Option<ResultantWorkflowState> ResultantWorkflow,
    Option<Guid> EntityGuid,
    Option<string> CallbackQueryId,
    TlgInputDetails Details);