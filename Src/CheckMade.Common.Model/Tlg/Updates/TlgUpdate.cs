namespace CheckMade.Common.Model.Tlg.Updates;

public record TlgUpdate(
     TlgUserId UserId,
     TlgChatId ChatId,
     TlgBotType BotType,
     TlgUpdateType TlgUpdateType,
     TlgUpdateDetails Details);
     