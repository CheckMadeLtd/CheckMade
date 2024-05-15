using CheckMade.Common.LanguageExtensions.MonadicWrappers;

namespace CheckMade.Common.Tests;

public class MonadicWrappersTests
{
    // Example unit tests for `Attempt`
    [Fact]
    public void Attempt_SelectMany_Sync()
    {
        var attempt1 = Attempt<int>.FromValue(2);
        var attempt2 = Attempt<int>.FromValue(3);

        var result = from x in attempt1
                     from y in attempt2
                     select x + y;

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public async Task Attempt_SelectMany_Async()
    {
        var attempt1 = Task.FromResult(Attempt<int>.FromValue(2));
        var attempt2 = Task.FromResult(Attempt<int>.FromValue(3));

        var result = await (from x in attempt1
                            from y in attempt2
                            select x + y);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    // Example unit tests for `Option`
    [Fact]
    public void Option_SelectMany_Sync()
    {
        var option1 = Option<int>.Some(2);
        var option2 = Option<int>.Some(3);

        var result = from x in option1
                     from y in option2
                     select x + y;

        Assert.True(result.IsSome);
        Assert.Equal(5, result.Value);
    }

    // Example unit tests for `Result`
    [Fact]
    public void Result_SelectMany_Sync()
    {
        var result1 = Result<int>.FromSuccess(2);
        var result2 = Result<int>.FromSuccess(3);

        var result = from x in result1
                     from y in result2
                     select x + y;

        Assert.True(result.Success);
        Assert.Equal(5, result.Value);
    }

    // Example unit tests for `Validation`
    [Fact]
    public void Validation_SelectMany_Sync()
    {
        var validation1 = Validation<int>.Valid(2);
        var validation2 = Validation<int>.Valid(3);

        var result = from x in validation1
                     from y in validation2
                     select x + y;

        Assert.True(result.IsValid);
        Assert.Equal(5, result.Value);
    }
}
