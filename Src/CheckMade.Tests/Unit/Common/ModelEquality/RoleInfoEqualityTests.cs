using CheckMade.Common.Model.Core.Actors.Concrete;
using CheckMade.Common.Model.Core.Actors.RoleSystem;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete;
using CheckMade.Common.Model.Core.Actors.RoleSystem.Concrete.RoleTypes;
using CheckMade.Common.Model.Core.LiveEvents;
using CheckMade.Common.Model.Core.LiveEvents.Concrete;
using CheckMade.Common.Model.Core.Trades.Concrete;
using CheckMade.Common.Model.Utils;

namespace CheckMade.Tests.Unit.Common.ModelEquality;

public class RoleInfoEqualityTests
{
    #region TestsThatPassOnlyThanksToCustomEqualityLogic

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentTypesImplementingIRoleInfo()
    {
        IRoleInfo roleInfo = new RoleInfo(
            "Token1",
            new TradeAdmin<SanitaryTrade>());

        IRoleInfo role = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        var areEqual1 = role.Equals(roleInfo);
        var areEqual2 = roleInfo.Equals(role);

        Assert.True(
            areEqual1,
            "RoleInfo and Role with the same data should be considered equal.");
        Assert.True(
            areEqual2,
            "RoleInfo and Role with the same data should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingRoles_WithDifferenceOnlyInUserAndLiveEvent()
    {
        IRoleInfo role1 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        IRoleInfo role2 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielDe),
            new LiveEventInfo(Y2024),
            new HashSet<ISphereOfAction>());

        var areEqual = role1.Equals(role2);

        Assert.True(
            areEqual,
            "Two Roles with the same token and type but different users and live events should still be considered equal.");
    }
    
    #endregion
    
    #region TestsThatWouldPassAlsoWithDefaultRecordEqualityComparison
    
    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingDifferentTypesImplementingIRoleInfo_WithEqualsOperator()
    {
        IRoleInfo roleInfo = new RoleInfo(
            "Token1",
            new TradeAdmin<SanitaryTrade>());

        IRoleInfo role = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        var areEqual = role == roleInfo;

        Assert.False(
            areEqual,
            "Equal RoleInfo and Role are not considered equal when using '==' instead of Equals()");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingDifferentTypesImplementingIRoleInfo_WithDifferingData()
    {
        IRoleInfo roleInfo = new RoleInfo(
            "Token1",
            new TradeAdmin<SanitaryTrade>());

        IRoleInfo role = new Role(
            "Token2",
            new TradeInspector<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        var areEqual1 = role.Equals(roleInfo);
        var areEqual2 = roleInfo.Equals(role);

        Assert.False(
            areEqual1,
            "RoleInfo and Role with different data should not be considered equal.");
        Assert.False(
            areEqual2,
            "RoleInfo and Role with different data should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingTheSameRoleInfoInstance()
    {
        IRoleInfo roleInfo1 = new RoleInfo(
            "Token1",
            new TradeAdmin<SanitaryTrade>());

        var roleInfo2 = roleInfo1;

        var areEqual = roleInfo1.Equals(roleInfo2);

        Assert.True(
            areEqual,
            "Two references to the same RoleInfo instance should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingTheSameRoleInstance()
    {
        IRoleInfo role1 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        var role2 = role1;

        var areEqual = role1.Equals(role2);

        Assert.True(
            areEqual,
            "Two references to the same Role instance should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingRoleInstanceToNull()
    {
        IRoleInfo role1 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        IRoleInfo? role2 = null;

        var areEqual = role1.Equals(role2);

        Assert.False(
            areEqual,
            "Comparing an actual Role to null should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingRoleInfoInstanceToNull()
    {
        IRoleInfo roleInfo1 = new RoleInfo(
            "Token1",
            new TradeAdmin<SanitaryTrade>());

        IRoleInfo? roleInfo2 = null;

        var areEqual = roleInfo1.Equals(roleInfo2);

        Assert.False(
            areEqual,
            "Comparing an actual RoleInfo to null should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentRoleInfoInstancesWithSameBasicData()
    {
        IRoleInfo roleInfo1 = new RoleInfo(
            "Token1",
            new TradeAdmin<SanitaryTrade>());

        IRoleInfo roleInfo2 = new RoleInfo(
            "Token1",
            new TradeAdmin<SanitaryTrade>());

        var areEqual = roleInfo1.Equals(roleInfo2);

        Assert.True(
            areEqual,
            "Two RoleInfo instances with the same basic data should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentRoleInstancesWithSameBasicData()
    {
        IRoleInfo role1 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        IRoleInfo role2 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        var areEqual = role1.Equals(role2);

        Assert.True(
            areEqual,
            "Two Role instances with the same basic data should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingRolesWithDifferentStatus()
    {
        IRoleInfo role1 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>());

        IRoleInfo role2 = new Role(
            "Token1",
            new TradeAdmin<SanitaryTrade>(),
            new UserInfo(DanielEn),
            new LiveEventInfo(X2024),
            new HashSet<ISphereOfAction>(),
            DbRecordStatus.Historic);

        var areEqual = role1.Equals(role2);

        Assert.False(
            areEqual,
            "Two Roles with different Status but otherwise identical data should not be considered equal.");
    }
    
    #endregion
}