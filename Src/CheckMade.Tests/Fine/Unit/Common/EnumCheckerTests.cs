using CheckMade.Common.Model.ChatBot.UserInteraction;
using CheckMade.Common.Model.Core.SanitaryOps.Issues;
using CheckMade.Common.Utils.Generic;
using Xunit.Abstractions;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace CheckMade.Tests.Fine.Unit.Common;

public class EnumCheckerTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public void IsDefined_ShouldBeTrue_ForDefinedEnum_InNormalEnum()
    {
        Assert.True(EnumChecker.IsDefined(ConsumablesIssue.Item.ToiletPaper));
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
    
    [Fact]
    public void IsDefined_ShouldBeFalse_ForUndefinedEnum_InControlPrompts()
    {
        Assert.False(EnumChecker.IsDefined(ControlPrompts.Back + 1));
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
        var combinedEnum = ControlPrompts.Good;
        combinedEnum ^= ControlPrompts.Ok;
        Assert.True((combinedEnum & ControlPrompts.Ok) != 0);
        combinedEnum ^= ControlPrompts.Ok;
        Assert.False((combinedEnum & ControlPrompts.Ok) != 0);
    }
}
