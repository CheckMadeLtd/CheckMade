namespace CheckMade.Common.Model.Tlg.Input;

public record TlgUpdate(
     TlgUserId UserId,
     TlgChatId ChatId,
     TlgBotType BotType,
     TlgUpdateType TlgUpdateType,
     TlgUpdateDetails Details);
     