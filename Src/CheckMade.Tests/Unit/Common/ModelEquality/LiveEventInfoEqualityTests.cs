using CheckMade.Common.DomainModel.Data.Core;
using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem;
using CheckMade.Common.DomainModel.Data.Core.Actors.RoleSystem.RoleTypes;
using CheckMade.Common.DomainModel.Data.Core.LiveEvents;
using CheckMade.Common.DomainModel.Data.Core.Trades;
using CheckMade.Common.DomainModel.Interfaces.Data.Core;

namespace CheckMade.Tests.Unit.Common.ModelEquality;

public sealed class LiveEventInfoEqualityTests
{
    /*
     * This region contains tests, the equivalents of which are not in this region for IRoleInfo.
     * This shows the limitations of the C# default value-comparison for records. It breaks down with
     * navigational properties for collections, here List<IRoleInfo>. For IRoleInfo this is not true because both
     * navigational properties are NOT collections but simple types. 
     */

    #region TestsThatPassOnlyThanksToCustomEqualityLogic
    
    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentTypesImplementingILiveEventInfo()
    {
        ILiveEventInfo liveEventInfo = new LiveEventInfo(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            DbRecordStatus.Active);

        ILiveEventInfo liveEvent = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        var areEqual1 = liveEvent.Equals(liveEventInfo);
        var areEqual2 = liveEventInfo.Equals(liveEvent);

        Assert.True(
            areEqual1,
            "LiveEventInfo and LiveEvent with the same data should be considered equal.");
        Assert.True(
            areEqual2,
            "LiveEventInfo and LiveEvent with the same data should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingLiveEvents_WithDifferenceOnlyInVenue()
    {
        ILiveEventInfo liveEvent1 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue 1"),
            new List<ISphereOfAction>());

        ILiveEventInfo liveEvent2 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue 2"),
            new List<ISphereOfAction>());

        var areEqual = liveEvent1.Equals(liveEvent2);

        Assert.True(
            areEqual,
            "Two LiveEvents with the same data but different venues should still be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentLiveEventInstancesWithSameBasicData()
    {
        ILiveEventInfo liveEvent1 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        ILiveEventInfo liveEvent2 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        var areEqual = liveEvent1.Equals(liveEvent2);

        Assert.True(
            areEqual,
            "Two LiveEvent instances with the same basic data should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingLiveEventsWithDifferentRoleInfoCollections()
    {
        var roleInfo1 = new RoleInfo("Token1", new TradeAdmin<SanitaryTrade>());
        var roleInfo2 = new RoleInfo("Token2", new TradeInspector<SanitaryTrade>());

        ILiveEventInfo liveEvent1 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo> { roleInfo1 },
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        ILiveEventInfo liveEvent2 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo> { roleInfo2 },
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        var areEqual = liveEvent1.Equals(liveEvent2);

        Assert.True(
            areEqual,
            "Two LiveEvents with different RoleInfo collections but otherwise identical data should " +
            "be considered equal.");
    }

    #endregion

    #region TestsThatWouldPassAlsoWithDefaultRecordEqualityComparison

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingDifferentTypesImplementingILiveEventInfo_WithEqualsOperator()
    {
        ILiveEventInfo liveEventInfo = new LiveEventInfo(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            DbRecordStatus.Active);

        ILiveEventInfo liveEvent = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        var areEqual = liveEvent == liveEventInfo; // shows that we need to use .Equals() instead of '=='

        Assert.False(
            areEqual,
            "Equal LiveEventInfo and LiveEvent are not considered equal when using '==' instead of Equals()");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingDifferentTypesImplementingILiveEventInfo_WithDifferingData()
    {
        ILiveEventInfo liveEventInfo = new LiveEventInfo(
            "Test Event 1",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            DbRecordStatus.Active);

        ILiveEventInfo liveEvent = new LiveEvent(
            "Test Event 2",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        var areEqual1 = liveEvent.Equals(liveEventInfo);
        var areEqual2 = liveEventInfo.Equals(liveEvent);

        Assert.False(
            areEqual1,
            "LiveEventInfo and LiveEvent with different data should not be considered equal.");
        Assert.False(
            areEqual2,
            "LiveEventInfo and LiveEvent with different data should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingTheSameLiveEventInfoInstance()
    {
        ILiveEventInfo liveEventInfo1 = new LiveEventInfo(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            DbRecordStatus.Active);

        var liveEventInfo2 = liveEventInfo1;

        var areEqual = liveEventInfo1.Equals(liveEventInfo2);

        Assert.True(
            areEqual,
            "Two references to the same LiveEventInfo instance should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingTheSameLiveEventInstance()
    {
        ILiveEventInfo liveEvent1 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        var liveEvent2 = liveEvent1;

        var areEqual = liveEvent1.Equals(liveEvent2);

        Assert.True(
            areEqual,
            "Two references to the same LiveEvent instance should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingLiveEventInstanceToNull()
    {
        ILiveEventInfo liveEvent1 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>());

        ILiveEventInfo? liveEvent2 = null;

        var areEqual = liveEvent1.Equals(liveEvent2);

        Assert.False(
            areEqual,
            "Comparing an actual LiveEvent to null should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingLiveEventInfoInstanceToNull()
    {
        ILiveEventInfo liveEvent1 = new LiveEventInfo(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5));

        ILiveEventInfo? liveEvent2 = null;

        var areEqual = liveEvent1.Equals(liveEvent2);

        Assert.False(
            areEqual,
            "Comparing an actual LiveEventInfo to null should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentLiveEventInfoInstancesWithSameBasicData()
    {
        ILiveEventInfo liveEventInfo1 = new LiveEventInfo(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            DbRecordStatus.Active);

        ILiveEventInfo liveEventInfo2 = new LiveEventInfo(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            DbRecordStatus.Active);

        var areEqual = liveEventInfo1.Equals(liveEventInfo2);

        Assert.True(
            areEqual,
            "Two LiveEventInfo instances with the same basic data should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingLiveEventsWithDifferentStatus()
    {
        ILiveEventInfo liveEvent1 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>(),
            DbRecordStatus.Active);

        ILiveEventInfo liveEvent2 = new LiveEvent(
            "Test Event",
            new DateTime(2024, 7, 1),
            new DateTime(2024, 7, 5),
            new List<IRoleInfo>(),
            new LiveEventVenue("Test Venue"),
            new List<ISphereOfAction>(),
            DbRecordStatus.Historic);

        var areEqual = liveEvent1.Equals(liveEvent2);

        Assert.False(
            areEqual,
            "Two LiveEvents with different Status but otherwise identical data should not be considered equal.");
    }
    
    #endregion
}