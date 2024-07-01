namespace CheckMade.Tests.Unit.Common;

public class EnumerableExtensionsTests
{
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsElementsFromTheEndUpToStopConditionIncluding_WhenInclusive()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);
        
        Assert.Equal([3, 4, 5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsElementsFromTheEndUpToStopConditionExcluding_WhenNotInclusive()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, false);
    
        Assert.Equal([4, 5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsAllElements_WhenStopConditionItemNotPresent()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = x => x == 7;
        var result = list.GetLatestRecordsUpTo(stopCondition, false);
    
        Assert.Equal(list, result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsNoElements_WhenStopConditionItemIsLastOne_Excluding()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = x => x == 5;
        var result = list.GetLatestRecordsUpTo(stopCondition, false);
    
        Assert.Equal([], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsLastElement_WhenStopConditionItemIsLastOne_Including()
    {
        List<int> list = [1, 2, 3, 4, 5];
        Func<int, bool> stopCondition = x => x == 5;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);
    
        Assert.Equal([5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsOnlyElement_WhenItIsStopConditionItem_Including()
    {
        List<int> list = [5];
        Func<int, bool> stopCondition = x => x == 5;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);
    
        Assert.Equal([5], result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsNoElements_WhenInputListIsEmpty()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        List<int> list = [];
        Func<int, bool> stopCondition = x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);

        Assert.Equal(list, result);
    }
    
    [Fact]
    public void GetLatestRecordsUpTo_ReturnsCorrectElements_WhenMultipleStopConditionsPresent()
    {
        List<int> list = [1, 3, 2, 3, 4, 5];
        Func<int, bool> stopCondition = x => x == 3;
        var result = list.GetLatestRecordsUpTo(stopCondition, true);

        Assert.Equal([3, 4, 5], result);
    }
}