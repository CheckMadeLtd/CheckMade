using CheckMade.Abstract.Domain.Data.Core;
using CheckMade.Abstract.Domain.Data.Core.Actors;
using CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem;
using CheckMade.Abstract.Domain.Data.Core.Actors.RoleSystem.RoleTypes;
using CheckMade.Abstract.Domain.Data.Core.Trades;
using CheckMade.Abstract.Domain.Interfaces.Data.Core;
using General.Utils.FpExtensions.Monads;
using General.Utils.UiTranslation;

namespace CheckMade.Tests.Unit.Abstract.Domain.ModelEquality;

public class UserInfoEqualityTests
{
    // See comment in the same place for RoleInfoEqualityTests!
    #region TestsThatPassOnlyThanksToCustomEqualityLogic

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentTypesImplementingIUserInfo()
    {
        IUserInfo userInfo = new UserInfo(
            new MobileNumber("+1234567890"),
            "John",
            "SomeMiddleName",
            "Doe",
            new EmailAddress("johndoe@checkmade.io"),
            LanguageCode.en);

        IUserInfo user = new User(
            new MobileNumber("+1234567890"),
            "John",
            "SomeMiddleName",
            "Doe",
            new EmailAddress("johndoe@checkmade.io"),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        var areEqual1 = user.Equals(userInfo);
        var areEqual2 = userInfo.Equals(user);

        Assert.True(
            areEqual1,
            "UserInfo and User with the same data should be considered equal.");
        Assert.True(
            areEqual2,
            "UserInfo and User with the same data should be considered equal.");
    }
    
    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingUsers_WithDifferenceOnlyInRoles()
    {
        IUserInfo user1 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>
            {
                new RoleInfo("Token1",
                    new TradeAdmin<SanitaryTrade>())
            },
            Option<Vendor>.None());

        IUserInfo user2 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>
            {
                new RoleInfo("Token2", 
                    new TradeInspector<SanitaryTrade>())
            },
            Option<Vendor>.None());

        var areEqual = user1.Equals(user2);

        Assert.True(
            areEqual,
            "Two Users with the same data but different roles should still be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentUserInstancesWithSameBasicData()
    {
        IUserInfo user1 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        IUserInfo user2 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        var areEqual = user1.Equals(user2);

        Assert.True(
            areEqual,
            "Two User instances with the same basic data should be considered equal.");
    }

    #endregion

    #region TestsThatWouldPassAlsoWithDefaultRecordEqualityComparison

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingDifferentTypesImplementingIUserInfo_WithEqualsOperator()
    {
        IUserInfo userInfo = new UserInfo(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en);

        IUserInfo user = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        var areEqual = user == userInfo;

        Assert.False(
            areEqual,
            "Equal UserInfo and User are not considered equal when using '==' instead of Equals()");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingDifferentTypesImplementingIUserInfo_WithDifferingData()
    {
        IUserInfo userInfo = new UserInfo(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en);

        IUserInfo user = new User(
            new MobileNumber("+0987654321"),
            "Jane",
            Option<string>.None(),
            "Smith",
            Option<EmailAddress>.None(),
            LanguageCode.de,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        var areEqual1 = user.Equals(userInfo);
        var areEqual2 = userInfo.Equals(user);

        Assert.False(
            areEqual1,
            "UserInfo and User with different data should not be considered equal.");
        Assert.False(
            areEqual2,
            "UserInfo and User with different data should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingTheSameUserInfoInstance()
    {
        IUserInfo userInfo1 = new UserInfo(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en);

        var userInfo2 = userInfo1;

        var areEqual = userInfo1.Equals(userInfo2);

        Assert.True(
            areEqual,
            "Two references to the same UserInfo instance should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingTheSameUserInstance()
    {
        IUserInfo user1 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        var user2 = user1;

        var areEqual = user1.Equals(user2);

        Assert.True(
            areEqual,
            "Two references to the same User instance should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingUserInstanceToNull()
    {
        IUserInfo user1 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        IUserInfo? user2 = null;

        var areEqual = user1.Equals(user2);

        Assert.False(
            areEqual,
            "Comparing an actual User to null should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingUserInfoInstanceToNull()
    {
        IUserInfo userInfo1 = new UserInfo(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en);

        IUserInfo? userInfo2 = null;

        var areEqual = userInfo1.Equals(userInfo2);

        Assert.False(
            areEqual,
            "Comparing an actual UserInfo to null should not be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenComparingDifferentUserInfoInstancesWithSameBasicData()
    {
        IUserInfo userInfo1 = new UserInfo(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en);

        IUserInfo userInfo2 = new UserInfo(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en);

        var areEqual = userInfo1.Equals(userInfo2);

        Assert.True(
            areEqual,
            "Two UserInfo instances with the same basic data should be considered equal.");
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingUsersWithDifferentStatus()
    {
        IUserInfo user1 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None());

        IUserInfo user2 = new User(
            new MobileNumber("+1234567890"),
            "John",
            Option<string>.None(),
            "Doe",
            Option<EmailAddress>.None(),
            LanguageCode.en,
            new List<IRoleInfo>(),
            Option<Vendor>.None(),
            DbRecordStatus.Historic);

        var areEqual = user1.Equals(user2);

        Assert.False(
            areEqual,
            "Two Users with different Status but otherwise identical data should not be considered equal.");
    }    
    
    #endregion
}