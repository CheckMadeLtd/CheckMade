namespace CheckMade.Common.Model.ChatBot.UserInteraction;

[Flags]
public enum ControlPrompts : long
{
    Back = 1L<<1,
    Cancel = 1L<<2,
    Skip = 1L<<3,
    Save = 1L<<4,
    SaveSkip = Save | Skip,
    Submit = 1L<<5,
    Review = 1L<<6,
    Edit = 1L<<7,
    Wait = 1L<<8,
    // Placeholder <<9
    // Placeholder <<10
    // Placeholder <<11
    // Placeholder <<12
    // Placeholder <<13
    
    No = 1L<<14,
    Yes = 1L<<15,
    Maybe = 1L<<16,
    YesNo = Yes | No,
    // Placeholder <<17
    // Placeholder <<18
    
    Bad = 1L<<19,
    Ok = 1L<<20,
    Good = 1L<<21,
    // Placeholder <<22
    // Placeholder <<23
    
}