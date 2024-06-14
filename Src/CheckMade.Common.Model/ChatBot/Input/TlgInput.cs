using CheckMade.Common.Model.ChatBot.UserInteraction;

namespace CheckMade.Common.Model.ChatBot.Input;

public record TlgInput(
     TlgUserId UserId,
     TlgChatId ChatId,
     InteractionMode InteractionMode,
     TlgInputType TlgInputType,
     TlgInputDetails Details);
     