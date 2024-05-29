namespace CheckMade.Common.Model;

public enum ControlPrompts : long
{
    Back = 1L,
    Cancel = 1L<<1,
    Skip = 1L<<2,
    Save = 1L<<3,
    Submit = 1L<<4,
    Review = 1L<<5,
    Edit = 1L<<6,
    // Placeholder <<7
    
    No = 1L<<8,
    Yes = 1L<<9,
    Maybe = 1L<<10,
    // Placeholder <<11
    
    Bad = 1L<<12,
    Ok = 1L<<13,
    Good = 1L<<14,
    // Placeholder <<15
    // Placeholder <<16
}