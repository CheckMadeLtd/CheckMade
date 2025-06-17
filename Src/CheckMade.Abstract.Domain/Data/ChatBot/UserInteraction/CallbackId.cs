using General.Utils.Validators;

namespace CheckMade.Abstract.Domain.Data.ChatBot.UserInteraction;

public sealed record CallbackId
{
    public string Id { get; }

    public CallbackId(long id)
    {
        if (id < 1)
        {
            throw new ArgumentException("ID must be a positive long integer.");
        }
        
        Id = id.ToString();
    }

    public CallbackId(string id)
    {
        if (!id.IsValidToken())
        {
            throw new ArgumentException("ID must be a 6-digit alphanumeric code starting with 'D'.");
        }
        
        Id = id;
    }

    public static implicit operator string(CallbackId callbackId) => callbackId.Id;
}