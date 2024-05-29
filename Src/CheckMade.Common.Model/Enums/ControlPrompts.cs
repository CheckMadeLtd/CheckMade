namespace CheckMade.Common.Model.Enums;

public enum ControlPrompts : long
{
    Back = 1L,
    Cancel = 1L<<1,
    Skip = 1L<<2,
    Save = 1L<<3,
    Submit = 1L<<4,
    Review = 1L<<5,
    Edit = 1L<<6,
    Wait = 1L<<7,
    // Placeholder <<8
    // Placeholder <<9
    
    No = 1L<<10,
    Yes = 1L<<11,
    Maybe = 1L<<12,
    // Placeholder <<13
    // Placeholder <<14
    
    Bad = 1L<<15,
    Ok = 1L<<16,
    Good = 1L<<17,
    // Placeholder <<18
    // Placeholder <<19
}