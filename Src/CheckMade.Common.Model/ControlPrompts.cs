namespace CheckMade.Common.Model;

public enum ControlPrompts : long
{
    Back = 1L,
    Cancel = 1L<<1,
    Save = 1L<<2,
    Submit = 1L<<3,
    Edit = 1L<<4,
    // Placeholder <<5
    
    No = 1L<<6,
    Yes = 1L<<7,
    Maybe = 1L<<8,
    // Placeholder <<9
    
    Bad = 1L<<9,
    Ok = 1L<<10,
    Good = 1L<<11,
    // Placeholder <<12
    // Placeholder <<13
}