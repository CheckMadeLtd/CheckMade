using CheckMade.Common.FpExt.MonadicWrappers;
using FluentAssertions;

namespace CheckMade.Common.Tests.MonadicWrappers;

public class OptionTests
{
    [Fact]
    public void TestOption_Match_Some()
    {
        var option = Option<int>.Some(5);
        var result = option.Match(
            onSome: value => value * 2,
            onNone: () => 0);

        result.Should().Be(10);
    }

    [Fact]
    public void TestOption_Match_None()
    {
        var option = Option<int>.None();
        var result = option.Match(
            onSome: value => value * 2,
            onNone: () => 0);

        result.Should().Be(0);
    }

    [Fact]
    public void TestOption_GetValueOrThrow_Some()
    {
        var option = Option<int>.Some(5);
        var value = option.GetValueOrThrow();

        value.Should().Be(5);
    }

    [Fact]
    public void TestOption_GetValueOrThrow_None()
    {
        var option = Option<int>.None();

        Action action = () => option.GetValueOrThrow();
        action.Should().Throw<InvalidOperationException>().WithMessage("No value present");
    }

    [Fact]
    public void TestOption_GetValueOrDefault_Some()
    {
        var option = Option<int>.Some(5);
        var value = option.GetValueOrDefault(10);

        value.Should().Be(5);
    }

    [Fact]
    public void TestOption_GetValueOrDefault_None()
    {
        var option = Option<int>.None();
        var value = option.GetValueOrDefault(10);

        value.Should().Be(10);
    }

    [Fact]
    public void TestSelectMany_SomeToSome()
    {
        /* This test demonstrates binding a Some Option<T> to another Some Option<TResult>.
         The initial value is Some, and the binder function also returns a Some Option<TResult>. */

        var source = Option<int>.Some(5);
        var result = source.SelectMany(Binder);

        result.IsSome.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void TestSelectMany_SomeToNone()
    {
        /* This test demonstrates binding a Some Option<T> to a None Option<TResult>.
         The initial value is Some, but the binder function returns a None Option<TResult>. */

        var source = Option<int>.Some(5);
        var result = source.SelectMany(BinderNone);

        result.IsNone.Should().BeTrue();
    }

    [Fact]
    public void TestSelectMany_NoneToBinding()
    {
        /* This test demonstrates binding a None Option<T> to any Option<TResult>.
         The initial value is None, and we expect the None to propagate without calling the binder function. */

        var source = Option<int>.None();
        var result = source.SelectMany(Binder);

        result.IsNone.Should().BeTrue();
    }

    private static Option<int> Binder(int i) => Option<int>.Some(i + 5);
    private static Option<int> BinderNone(int i) => Option<int>.None();
    
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
    
    [Fact]
    public async Task TestSelectMany_SourceTaskThrowsException()
    {
        /* This test shows what happens when the initial operation—the source Task—throws an exception.
        You can expect this to propagate up to the calling code, which the test verifies. */

        Task<Option<int>> faultedTask = Task.FromException<Option<int>>(new Exception("Simulated exception"));

        var action = async () => { await faultedTask; };

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
    }

    [Fact]
    public async Task TestSelectMany_CollectionTaskThrowsException()
    {
        /* Similar to the previous test, except this time the exception originates from the subsequent operation.
        This test checks whether exceptions inside async operations are correctly propagated. */

        var sourceTask = Task.FromResult(Option<int>.Some(5));

        Func<Task> action = async () =>
            await (from s in await sourceTask from c in FaultedSelector() select c + s);

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
        return;

        static async Task<Option<int>> FaultedSelector() => 
            await Task.FromException<Option<int>>(new Exception("Simulated exception"));
    }
    
    [Fact]
    public void TestSelectMany_SyncBindingToNone()
    {
        /* This test simulates a situation where the source value is `None`. We can expect that no matter 
        what operation we try to apply, the result should also be `None`. */

        var source = Option<int>.None();

        var result = 
            from s in source 
            from res in Binder(s)
            select res;

        result.Should().Be(Option<int>.None());
        return;

        static Option<int> Binder(int i) => Option<int>.Some(i + 5);
    }

    [Fact]
    public async Task TestSelectMany_AsyncBindingToNone()
    {
        /* The same scenario as the previous test, but with the source value provided by a Task. Again, 
        when the source is `None`, the result is expected to be `None`. */
        
        var sourceTask = Task.FromResult(Option<int>.None());

        var result = 
            await (from s in await sourceTask from c in CollectionTaskSelector(s) select c + s);

        result.Should().Be(Option<int>.None());
        return;

        static async Task<Option<int>> CollectionTaskSelector(int i) => 
            await Task.FromResult(Option<int>.Some(i + 5));
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncSourceNoneDoesNotCallSelector()
    {
        var sourceTask = Task.FromResult(Option<int>.None());
        var selectorWasCalled = false;

        var result = await (from s in await sourceTask
            from c in CollectionTaskSelector(s)
            select c + s);

        result.Should().Be(Option<int>.None());
        selectorWasCalled.Should().BeFalse();

        return;

        async Task<Option<int>> CollectionTaskSelector(int i)
        {
            selectorWasCalled = true;
            return await Task.FromResult(Option<int>.Some(i + 5));
        }
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps2()
    {
        /* This test demonstrates the binding of asynchronous operations in sequence.
         It starts with an asynchronous operation (sourceTask) and binds it to another asynchronous operation
         (collectionTaskSelector), resulting in a final transformation using the resultSelector function. */

        var sourceTask = Task.FromResult(Option<int>.Some(5));

        var result = await sourceTask.SelectMany(
            async s => await CollectionTaskSelector(s),
            (s, c) => s + c
        );

        result.Should().BeEquivalentTo(Option<int>.Some(15));
        return;

        static async Task<Option<int>> CollectionTaskSelector(int i) => await Task.FromResult(Option<int>.Some(i + 5));
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp2()
    {
        /* This test illustrates the binding of an asynchronous initial operation to a synchronous subsequent operation.
         It begins with an asynchronous operation (sourceTask) and binds it to a synchronous operation 
         (collectionSelector), resulting in a final transformation using the resultSelector function. */

        var sourceTask = Task.FromResult(Option<int>.Some(5));

        var result = await sourceTask.SelectMany(
            s => CollectionSelector(s),
            (s, c) => s + c
        );

        result.Should().BeEquivalentTo(Option<int>.Some(15));
        return;

        static Option<int> CollectionSelector(int i) => Option<int>.Some(i + 5);
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps_SourceNone()
    {
        /* This test covers the scenario where the initial asynchronous operation returns None.
        Even though the operations are asynchronous, the final result should also be None. */

        var sourceTask = Task.FromResult(Option<int>.None());

        var result = await sourceTask.SelectMany(
            async s => await CollectionTaskSelector(s),
            (s, c) => s + c
        );

        result.Should().Be(Option<int>.None());
        return;

        static async Task<Option<int>> CollectionTaskSelector(int i) => await Task.FromResult(Option<int>.Some(i + 5));
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_SourceNone()
    {
        /* This test examines the behavior when the initial asynchronous operation returns None.
        The subsequent synchronous operation should not be called, and the final result should be None. */

        var sourceTask = Task.FromResult(Option<int>.None());

        var result = await sourceTask.SelectMany(
            s => CollectionSelector(s),
            (s, c) => s + c
        );

        result.Should().Be(Option<int>.None());
        return;

        static Option<int> CollectionSelector(int i) => Option<int>.Some(i + 5);
    }

    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps_Exception()
    {
        /* This test checks the behavior when the subsequent asynchronous operation throws an exception.
        The test ensures that the exception is correctly propagated. */

        var sourceTask = Task.FromResult(Option<int>.Some(5));

        Func<Task> action = async () =>
            await sourceTask.SelectMany(
                async _ => await FaultedSelector(),
                (s, c) => s + c
            );

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
        return;

        static async Task<Option<int>> FaultedSelector() => 
            await Task.FromException<Option<int>>(new Exception("Simulated exception"));
    }

    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_Exception()
    {
        /* This test ensures that when the synchronous subsequent operation throws an exception,
        it is correctly propagated, even if the initial operation was asynchronous. */

        var sourceTask = Task.FromResult(Option<int>.Some(5));

        Func<Task> action = async () =>
            await sourceTask.SelectMany(
                _ => FaultedSelector(),
                (s, c) => s + c
            );

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
        return;

        static Option<int> FaultedSelector() => 
            throw new Exception("Simulated exception");
    }
    
    [Fact]
    public async Task TestSelectMany_ChainingMultipleOperations()
    {
        /* This test demonstrates chaining multiple operations together.
         It starts with an initial value and performs a series of synchronous and asynchronous operations. */

        var source = Option<int>.Some(5);

        var intermediateResult = await source
            .SelectMany(s => Task.FromResult(SyncOperation1(s)), (_, a) => a)
            .SelectMany(a => AsyncOperation2(a), (_, b) => b);

        var result = await intermediateResult
            .SelectMany(b => Task.FromResult(SyncOperation3(b)), (_, c) => Task.FromResult(c));

        result.Should().BeEquivalentTo(Option<int>.Some(20));
        return;

        static Option<int> SyncOperation1(int i) => Option<int>.Some(i + 5);
        static async Task<Option<int>> AsyncOperation2(int i) => await Task.FromResult(Option<int>.Some(i * 2));
        static Option<int> SyncOperation3(int i) => Option<int>.Some(i - 5);
    }
    
    [Fact]
    public async Task TestSelectMany_NestedOptionTypes()
    {
        /* This test ensures that nested Option types are handled correctly.
         The source is an Option<Option<int>>, and the operations are performed on the inner value. */

        var source = Option<Option<int>>.Some(Option<int>.Some(5));

        var result = await source.SelectMany(
            outer => Task.FromResult(outer),
            (_, inner) => inner + 5
        );

        result.Should().BeEquivalentTo(Option<int>.Some(10));
    }
    
    [Fact]
    public async Task TestSelectMany_ComplexTypes()
    {
        /* This test demonstrates using more complex types instead of simple int values.
         It ensures that the Option class works correctly with complex data types. */

        var source = Option<Person>.Some(new Person { Name = "Alice", Age = 30 });

        var result = await source.SelectMany(
            async p => await UpdateAgeAsync(p),
            (p, updatedAge) => new Person { Name = p.Name, Age = updatedAge }
        );

        result.Should().BeEquivalentTo(Option<Person>.Some(new Person { Name = "Alice", Age = 35 }));
        return;

        static async Task<Option<int>> UpdateAgeAsync(Person person)
        {
            return await Task.FromResult(Option<int>.Some(person.Age + 5));
        }
    }

    private record Person
    {
        public string? Name { get; init; }
        public int Age { get; init; }
    }
}