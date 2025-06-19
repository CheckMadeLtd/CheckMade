namespace CheckMade.Abstract.Domain.Data.Bot;

public sealed record MessageId(int Id)
{
    public static implicit operator int(MessageId messageId) => messageId.Id;
    public static implicit operator MessageId(int id) => new(id);
}