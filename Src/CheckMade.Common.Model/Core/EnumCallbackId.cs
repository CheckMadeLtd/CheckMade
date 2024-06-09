namespace CheckMade.Common.Model.Core;

public record EnumCallbackId
{
    // To avoid clash between non-flagged Enum DomainCategory with flagged Enum ControlPrompt
    // These two Enums share one numeric space because they need to share Telegram.Update.CallbackQuery.Data field 
    public const int DomainCategoryMaxThreshold = 99999;
    
    public string Id { get; }

    public EnumCallbackId(long id)
    {
        if(id < 1)
        {
            throw new ArgumentException("ID must be a positive long integer.");
        }
        
        Id = id.ToString();
    }
}