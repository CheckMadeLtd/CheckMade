namespace CheckMade.Telegram.Model;

public record UserId(long Id)
{
    public static implicit operator long(UserId userId) => userId.Id;
    public static implicit operator UserId(long id) => new UserId(id);
}