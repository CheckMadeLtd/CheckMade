namespace CheckMade.Telegram.Model.BotPrompts;

public record BotPromptId
{
    public string Id { get; }

    public BotPromptId(int id)
    {
        if(id < 1)
        {
            throw new ArgumentException("ID must be a positive integer.");
        }
        
        Id = id.ToString();
    }
}