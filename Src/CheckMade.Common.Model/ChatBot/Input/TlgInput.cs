using CheckMade.Common.Model.Core.Interfaces;

namespace CheckMade.Common.Model.ChatBot.Input;

public record TlgInput(
     TlgAgent TlgAgent,
     TlgInputType InputType,
     Option<IRoleInfo> OriginatorRole,
     Option<ILiveEventInfo> LiveEventContext,
     Option<ResultantWorkflowInfo> ResultantWorkflow,
     TlgInputDetails Details);
     