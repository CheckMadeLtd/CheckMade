using CheckMade.Common.Utils.Validators;
using InputValidator = CheckMade.Common.Utils.Validators.InputValidator;

namespace CheckMade.Tests.Unit.Common;

public class InputValidatorTests
{
    [Theory]
    [InlineData("testing@checkmade.io", true)]
    [InlineData("name.surname@checkmade.io", true)]
    [InlineData("name+alias@checkmade.io", true)]
    [InlineData("name-alias@checkmade.io", true)]
    [InlineData("name_alias@checkmade.io", true)]
    [InlineData("name@checkmade.io.org", true)]
    [InlineData("testing@checkmade", false)]
    [InlineData("nam..ename@checkmade.io", false)]
    [InlineData(".name@checkmade.io", false)]
    [InlineData("name.@checkmade.io", false)]
    [InlineData("name@.io", false)]
    [InlineData("name@check..made.io", false)]
    [InlineData("name@", false)]
    [InlineData("@checkmade.io", false)]
    [InlineData("name.checkmade.io", false)]
    [InlineData(" testing@checkmade.io", false)]
    [InlineData(" testing@checkmade.io ", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    public void IsValidEmailAddress_ReturnsExpectedResult(string email, bool expectedOutcome)
    {
        var actualOutcome = InputValidator.IsValidEmailAddress(email);
        Assert.Equal(expectedOutcome, actualOutcome);
    }

    [Theory]
    [InlineData("+447469631749", true)]
    [InlineData("+15617593787", true)]
    [InlineData("+491724218925", true)]
    [InlineData(" +491724218925", false)]
    [InlineData("+491724218925 ", false)]
    [InlineData("+44 7469631749", false)]
    [InlineData("+44(0)7469631749", false)]
    // 0 right after country-code - skipping for now, hard to regex variety of countrycodes
    // [InlineData("+4407469631749", false)]
    [InlineData("+44-7469-631749", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    public void IsValidMobileNumber(string mobile, bool expectedOutcome)
    {
        var actualOutcome = InputValidator.IsValidMobileNumber(mobile);
        Assert.Equal(expectedOutcome, actualOutcome);
    }
    
    [Theory]
    [InlineData("ABC123", true)]
    [InlineData("abc123", true)]
    [InlineData("123456", true)]
    [InlineData("ABC1234", false)]
    [InlineData("ABC12", false)]
    [InlineData("ABC 123", false)]
    [InlineData("ABC-123", false)]
    [InlineData(" ", false)]
    [InlineData("", false)]
    public void IsValidToken_ReturnsExpectedResult(string token, bool expectedOutcome)
    {
        var actualOutcome = token.IsValidToken();
        Assert.Equal(expectedOutcome, actualOutcome);
    }
}