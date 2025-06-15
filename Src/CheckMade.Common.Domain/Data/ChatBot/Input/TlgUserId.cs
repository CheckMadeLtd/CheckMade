namespace CheckMade.Common.Domain.Data.ChatBot.Input;

public sealed record TlgUserId(long Id)
{
    public static implicit operator long(TlgUserId userId) => userId.Id;
    public static implicit operator TlgUserId(long id) => new(id);
}