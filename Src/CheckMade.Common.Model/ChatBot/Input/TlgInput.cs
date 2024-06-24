using CheckMade.Common.Model.Core;

namespace CheckMade.Common.Model.ChatBot.Input;

public record TlgInput(
     TlgAgent TlgAgent,
     TlgInputType InputType,
     Option<RoleStub> OriginatorRole,
     Option<LiveEventStub> LiveEventContext,
     TlgInputDetails Details);
     