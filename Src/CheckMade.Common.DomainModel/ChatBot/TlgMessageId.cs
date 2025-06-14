namespace CheckMade.Common.DomainModel.ChatBot;

public sealed record TlgMessageId(int Id)
{
    public static implicit operator int(TlgMessageId messageId) => messageId.Id;
    public static implicit operator TlgMessageId(int id) => new(id);
}