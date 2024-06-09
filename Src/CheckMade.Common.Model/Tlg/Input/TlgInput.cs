using CheckMade.Common.Model.Enums.UserInteraction;

namespace CheckMade.Common.Model.Tlg.Input;

public record TlgInput(
     TlgUserId UserId,
     TlgChatId ChatId,
     InteractionMode InteractionMode,
     TlgInputType TlgInputType,
     TlgInputDetails Details);
     