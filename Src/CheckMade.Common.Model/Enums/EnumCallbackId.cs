namespace CheckMade.Common.Model.Enums;

public record EnumCallbackId
{
    public string Id { get; }

    public EnumCallbackId(int id)
    {
        if(id < 1)
        {
            throw new ArgumentException("ID must be a positive integer.");
        }
        
        Id = id.ToString();
    }
}