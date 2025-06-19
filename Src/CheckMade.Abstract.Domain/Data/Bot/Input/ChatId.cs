namespace CheckMade.Abstract.Domain.Data.Bot.Input;

public sealed record ChatId(long Id)
{
    public static implicit operator long(ChatId chatId) => chatId.Id;
    public static implicit operator ChatId(long id) => new(id);
}