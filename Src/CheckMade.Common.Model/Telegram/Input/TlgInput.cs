using CheckMade.Common.Model.Telegram.UserInteraction;

namespace CheckMade.Common.Model.Telegram.Input;

public record TlgInput(
     TlgUserId UserId,
     TlgChatId ChatId,
     InteractionMode InteractionMode,
     TlgInputType TlgInputType,
     TlgInputDetails Details);
     