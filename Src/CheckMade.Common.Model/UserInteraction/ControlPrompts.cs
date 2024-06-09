namespace CheckMade.Common.Model.UserInteraction;

[Flags]
public enum ControlPrompts : long
{
    // IMPORTANT: 1L<<17 (first 6-digit power of 2) is the minimum allowed, to avoid clash with DomainCategory Enum!
    // See also const DomainCategoryMaxThreshold
    
    Back = 1L<<17,
    Cancel = 1L<<18,
    BackCancel = Back | Cancel,
    Skip = 1L<<19,
    Save = 1L<<20,
    Submit = 1L<<21,
    Review = 1L<<22,
    Edit = 1L<<23,
    Wait = 1L<<24,
    // Placeholder <<25
    // Placeholder <<26
    
    No = 1L<<27,
    Yes = 1L<<28,
    Maybe = 1L<<29,
    // Placeholder <<30
    // Placeholder <<31
    
    Bad = 1L<<32,
    Ok = 1L<<33,
    Good = 1L<<34,
    // Placeholder <<35
    // Placeholder <<36
}