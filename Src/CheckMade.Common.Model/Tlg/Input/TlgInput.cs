namespace CheckMade.Common.Model.Tlg.Input;

public record TlgInput(
     TlgUserId UserId,
     TlgChatId ChatId,
     TlgBotType BotType,
     TlgInputType TlgInputType,
     TlgInputDetails Details);
     