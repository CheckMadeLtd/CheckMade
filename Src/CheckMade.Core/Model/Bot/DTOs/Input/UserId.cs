namespace CheckMade.Core.Model.Bot.DTOs.Input;

public sealed record UserId(long Id)
{
    public static implicit operator long(UserId userId) => userId.Id;
    public static implicit operator UserId(long id) => new(id);
}