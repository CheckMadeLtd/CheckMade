namespace CheckMade.Common.Model.ChatBot.Input;

public sealed record TlgChatId(long Id)
{
    public static implicit operator long(TlgChatId chatId) => chatId.Id;
    public static implicit operator TlgChatId(long id) => new(id);
}