namespace CheckMade.Common.Model.Tlg.Input;

public record TlgInput(
     TlgUserId UserId,
     TlgChatId ChatId,
     TlgInteractionMode InteractionMode,
     TlgInputType TlgInputType,
     TlgInputDetails Details);
     