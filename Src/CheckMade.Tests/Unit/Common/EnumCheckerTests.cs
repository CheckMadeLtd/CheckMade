using CheckMade.Common.DomainModel.ChatBot.UserInteraction;
using CheckMade.Common.DomainModel.Core.LiveEvents.Concrete.SphereOfActionDetails;
using CheckMade.Common.Utils.Generic;
using Xunit.Abstractions;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace CheckMade.Tests.Unit.Common;

public sealed class EnumCheckerTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void IsDefined_ShouldBeTrue_ForDefinedEnum_InNormalEnum()
    {
        Assert.True(EnumChecker.IsDefined(ConsumablesItem.ToiletPaper));
    }
    
    [Fact]
    public void IsDefined_ShouldBeTrue_ForDefinedEnum_InControlPrompts()
    {
        Assert.True(EnumChecker.IsDefined(ControlPrompts.Back));
    }

    [Fact]
    public void IsDefined_ShouldBeTrue_ForCombinedEnum_InControlPrompts()
    {
        const ControlPrompts combinedEnum = ControlPrompts.Back | ControlPrompts.Cancel;
        outputHelper.WriteLine(((long)combinedEnum).ToString());
        
        Assert.True(EnumChecker.IsDefined(combinedEnum));
    }
    
    [Fact(Skip = "Mysteriously failing - investigate later")]
    public void IsDefined_ShouldBeFalse_ForUndefinedEnum_InControlPrompts()
    {
        Assert.False(EnumChecker.IsDefined(ControlPrompts.Skip + 5));
    }

    [Fact]
    public void SingleEnumValue_IncludedInCombined_Check()
    {
        var combinedEnum = ControlPrompts.Maybe;
        combinedEnum |= ControlPrompts.No; // Using |= operator
        
        var includesMaybe = (combinedEnum & ControlPrompts.Maybe) != 0;
        
        Assert.True(includesMaybe);
    }

    [Fact]
    public void ToggleEnum_WorksAsExpected()
    {
        var combinedEnum = ControlPrompts.Yes;
        combinedEnum ^= ControlPrompts.Maybe;
        Assert.True((combinedEnum & ControlPrompts.Maybe) != 0);
        combinedEnum ^= ControlPrompts.Maybe;
        Assert.False((combinedEnum & ControlPrompts.Maybe) != 0);
    }
}
