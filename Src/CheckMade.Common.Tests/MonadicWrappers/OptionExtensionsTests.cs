using CheckMade.Common.FpExt.MonadicWrappers;
using FluentAssertions;

namespace CheckMade.Common.Tests.MonadicWrappers;

public class OptionExtensionsTests
{
    [Fact]
    public void TestSelectMany_SyncBindingSyncOps()
    {
        /* This test demonstrates synchronous binding of synchronous operations. It starts from an initial synchronous
         value (retrieved from the source variable), and then pipeline it to another synchronous operation such as 
         mapping, filtering, or flatmapping. 
         The key feature here is that all operations are done in a synchronous manner. */
        
        var source = Option<int>.Some(5);

        var result = 
            from s in source 
            from res in Binder(s)
            select res;

        result.Should().BeEquivalentTo(Option<int>.Some(10));
        return;

        static Option<int> Binder(int i) => Option<int>.Some(i + 5);
    }
    
    [Fact]
    public void TestSelectMany_CombineTwoSyncOps()
    {
        /* This test shows how to combine two synchronous operations to produce a final result.
         Here we have an additional function (collectionSelector) which operates on the source value and produces
         another Option<T>. The final result is obtained by combining the source and selected values. */
        
        var source = Option<int>.Some(5);

        var result = 
            from s in source 
            from c in CollectionSelector(s) 
            select s + c;

        result.Should().BeEquivalentTo(Option<int>.Some(15));
        return;

        static Option<int> CollectionSelector(int i) => Option<int>.Some(i + 5);
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps()
    {
        /* This test illustrates asynchronous binding of asynchronous operations.
         It starts with an asynchronous operation (sourceTask). Once the task resolves, the result is piped into 
         another asynchronous operation (collectionTaskSelector). 
         The await keyword is used to allow asynchronous execution of tasks. */
        
        var sourceTask = Task.FromResult(Option<int>.Some(5));

        var result = 
            await (
                from s in await sourceTask 
                from c in CollectionTaskSelector(s) 
                select c + s);

        result.Should().BeEquivalentTo(Option<int>.Some(15));
        return;

        static async Task<Option<int>> CollectionTaskSelector(int i) => 
            await Task.FromResult(Option<int>.Some(i + 5));
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp()
    {
        /* This unit test details the binding of an asynchronous initial operation to a synchronous subsequent
         operation. Initially, an asynchronous operation is called (sourceTask), and once this completes, its result 
         is passed to a synchronous operation (collectionSelector). 
         This example shows how to efficiently chain asynchronous and synchronous operations. */ 
        
        var sourceTask = Task.FromResult(Option<int>.Some(5));

        var result = 
            from s in await sourceTask 
            from c in CollectionSelector(s)
            select c + s;

        result.Should().BeEquivalentTo(Option<int>.Some(15));
        return;

        static Option<int> CollectionSelector(int i) => Option<int>.Some(i + 5);
    }
    
    [Fact]
    public async Task TestSelectMany_SyncInitOpBindingToAsyncSubsequentOp()
    {
        /* In this test, a synchronous operation initiates and binds to an asynchronous subsequent operation.
         A synchronous operation (source) is executed first, and its result is used in an asynchronous function 
         (collectionTaskSelector) to yield the final result. */
        
        var source = Option<int>.Some(5);

        var result = await (
            from s in source 
            from c in CollectionTaskSelector(s) 
            select c + s);

        result.Should().BeEquivalentTo(Option<int>.Some(15));
        return;

        static async Task<Option<int>> CollectionTaskSelector(int i) => await Task.FromResult(Option<int>.Some(i + 5));
    }
}