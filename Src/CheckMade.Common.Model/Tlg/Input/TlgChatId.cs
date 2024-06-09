namespace CheckMade.Common.Model.Tlg.Input;

public record TlgChatId(long Id)
{
    public static implicit operator long(TlgChatId chatId) => chatId.Id;
    public static implicit operator TlgChatId(long id) => new TlgChatId(id);
}