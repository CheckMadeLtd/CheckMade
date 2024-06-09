namespace CheckMade.Common.Model.Tlg.Updates;

public record TlgUserId(long Id)
{
    public static implicit operator long(TlgUserId userId) => userId.Id;
    public static implicit operator TlgUserId(long id) => new TlgUserId(id);
}