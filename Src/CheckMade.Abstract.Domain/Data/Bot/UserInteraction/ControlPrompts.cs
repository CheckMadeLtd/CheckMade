namespace CheckMade.Abstract.Domain.Data.Bot.UserInteraction;

[Flags]
public enum ControlPrompts : long
{
    Back = 1L,
    Cancel = 1L << 1,
    Skip = 1L << 2,
    Save = 1L << 3,
    Submit = 1L << 4,
    Review = 1L << 5,
    Edit = 1L << 6,
    Wait = 1L << 7,
    Continue = 1L << 8,
    // Placeholder << 9
    // Placeholder << 10
    // Placeholder << 11
    // Placeholder << 12
    // Placeholder << 13
    
    No = 1L << 14,
    Yes = 1L << 15,
    Maybe = 1L << 16,
    YesNo = Yes | No,
    // Placeholder << 17
    // Placeholder << 18
    
    ViewAttachments = 1L << 19
}