using CheckMade.Common.Model.Core.Enums;

namespace CheckMade.Common.Model.Tlg.Input;

public record TlgInput(
     TlgUserId UserId,
     TlgChatId ChatId,
     InteractionMode InteractionMode,
     TlgInputType TlgInputType,
     TlgInputDetails Details);
     