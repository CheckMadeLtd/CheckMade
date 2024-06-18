namespace CheckMade.Common.Model.Utils;

public record ControlPromptsCallbackId
{
    public string Id { get; }

    public ControlPromptsCallbackId(long id)
    {
        if(id < 1)
        {
            throw new ArgumentException("ID must be a positive long integer.");
        }
        
        Id = id.ToString();
    }
}