using FluentAssertions;

namespace CheckMade.Common.Tests.Unit.MonadicWrappers;

public class ValidationTests
{
    [Fact]
    public void TestValidation_Match_Valid()
    {
        var validation = Validation<int>.Valid(5);
        var result = validation.Match(
            onValid: value => value * 2,
            onInvalid: _ => 0);

        result.Should().Be(10);
    }

    [Fact]
    public void TestValidation_Match_Invalid()
    {
        var validation = Validation<int>.Invalid(UiNoTranslate("Error"));
        var result = validation.Match(
            onValid: value => value * 2,
            onInvalid: errors => errors.Count);

        result.Should().Be(1);
    }

    [Fact]
    public void TestValidation_GetValueOrThrow_Valid()
    {
        var validation = Validation<int>.Valid(5);
        var value = validation.GetValueOrThrow();

        value.Should().Be(5);
    }

    [Fact]
    public void TestValidation_GetValueOrThrow_Invalid()
    {
        var validation = Validation<int>.Invalid(UiNoTranslate("Error"));

        Action action = () => validation.GetValueOrThrow();
        action.Should().Throw<InvalidOperationException>().WithMessage("Error");
    }

    [Fact]
    public void TestValidation_GetValueOrDefault_Valid()
    {
        var validation = Validation<int>.Valid(5);
        var value = validation.GetValueOrDefault(10);

        value.Should().Be(5);
    }

    [Fact]
    public void TestValidation_GetValueOrDefault_Invalid()
    {
        var validation = Validation<int>.Invalid(UiNoTranslate("Error"));
        var value = validation.GetValueOrDefault(10);

        value.Should().Be(10);
    }
    
    [Fact]
    public void Select_ShouldReturnSuccessfulResult_WhenSourceIsValid()
    {
        // Arrange
        var source = Validation<int>.Valid(2);
        
        // Act
        var result = from s in source
            select s * 2;
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(4);
    }
    
    [Fact]
    public void Select_ShouldReturnFailureResult_WhenSourceIsInvalid()
    {
        // Arrange
        var source = Validation<int>.Invalid(UiNoTranslate("Error message"));
        
        // Act
        var result = from s in source
            select s * 2;
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UiNoTranslate("Error message"));
    }
    
    [Fact]
    public void Where_ShouldReturnSuccessfulResult_WhenSourceIsValidAndPredicateIsSatisfied()
    {
        // Arrange
        var source = Validation<int>.Valid(2);
        
        // Act
        var result = from s in source
            where s > 1
            select s;
        
        // Assert
        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(2);
    }
    
    [Fact]
    public void Where_ShouldReturnFailureResult_WhenSourceIsValidAndPredicateIsNotSatisfied()
    {
        // Arrange
        var source = Validation<int>.Valid(2);
        
        // Act
        var result = from s in source
            where s < 1
            select s;
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UiNoTranslate("Predicate not satisfied"));
    }
    
    [Fact]
    public void Where_ShouldReturnFailureResult_WhenSourceIsInvalid()
    {
        // Arrange
        var source = Validation<int>.Invalid(UiNoTranslate("Error message"));
        
        // Act
        var result = from s in source
            where s > 1
            select s;
        
        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(UiNoTranslate("Error message"));
    }
    
    [Fact]
    public void TestSelectMany_SuccessToSuccess()
    {
        /* This test demonstrates binding a valid Validation<T> to another valid Validation<TResult>.
         The initial value is valid, and the binder function also returns a valid Validation<TResult>. */

        var source = Validation<int>.Valid(5);
        var result = source.SelectMany(Binder);

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void TestSelectMany_SuccessToInvalid()
    {
        /* This test demonstrates binding a valid Validation<T> to an invalid Validation<TResult>.
         The initial value is valid, but the binder function returns an invalid Validation<TResult>. */

        var source = Validation<int>.Valid(5);
        var result = source.SelectMany(BinderInvalid);

        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(UiNoTranslate("Simulated error"));
    }

    private static Validation<int> BinderInvalid(int i) => Validation<int>.Invalid(UiNoTranslate("Simulated error"));
    
    [Fact]
    public void TestSelectMany_InvalidToBinding()
    {
        /* This test demonstrates binding an invalid Validation<T> to any Validation<TResult>.
         The initial value is invalid, and we expect the errors to propagate without calling the binder function. */

        var source = Validation<int>.Invalid(UiNoTranslate("Initial error"));
        var result = source.SelectMany(Binder);

        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(UiNoTranslate("Initial error"));
    }
    
    [Fact]
    public void TestSelectMany_SyncBindingSyncOps()
    {
        /* This test demonstrates synchronous binding of synchronous operations. It starts from an initial synchronous
         value (retrieved from the source variable), and then pipelines it to another synchronous operation such as 
         mapping, filtering, or flatmapping. 
         The key feature here is that all operations are done in a synchronous manner. */
        
        var source = Validation<int>.Valid(5);

        var result = from s in source from res in Binder(s) select res;

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(10);
    }
    
    [Fact]
    public void TestSelectMany_CombineTwoSyncOps()
    {
        /* This test shows how to combine two synchronous operations to produce a final result.
         Here we have an additional function (collectionSelector) which operates on the source value and produces
         another Validation<T>. The final result is obtained by combining the source and selected values. */
        
        var source = Validation<int>.Valid(5);

        var result = from s in source from c in CollectionSelector(s) select s + c;

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(11);
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps()
    {
        /* This test illustrates asynchronous binding of asynchronous operations.
         It starts with an asynchronous operation (sourceTask). Once the task resolves, the result is piped into 
         another asynchronous operation (collectionTaskSelector). 
         The await keyword is used to allow asynchronous execution of tasks. */
        
        var sourceTask = Task.FromResult(Validation<int>.Valid(5));

        var result = await (from s in await sourceTask from c in CollectionTaskSelector(s) select c + s);

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(15);
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp()
    {
        /* This unit test details the binding of an asynchronous initial operation to a synchronous subsequent
         operation. Initially, an asynchronous operation is called (sourceTask), and once this completes, its result 
         is passed to a synchronous operation (collectionSelector). 
         This example shows how to efficiently chain asynchronous and synchronous operations. */ 
        
        var sourceTask = Task.FromResult(Validation<int>.Valid(5));

        var result = from s in await sourceTask from c in CollectionSelector(s) select c + s;

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(11);
    }
    
    [Fact]
    public async Task TestSelectMany_SyncInitOpBindingToAsyncSubsequentOp()
    {
        /* In this test, a synchronous operation initiates and binds to an asynchronous subsequent operation.
         A synchronous operation (source) is executed first, and its result is used in an asynchronous function 
         (collectionTaskSelector) to yield the final result. */
        
        var source = Validation<int>.Valid(5);

        var result = await (from s in source from c in CollectionTaskSelector(s) select c + s);

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(15);
    }
    
    [Fact]
    public async Task TestSelectMany_SourceTaskThrowsException()
    {
        /* This test shows what happens when the initial operation—the source Task—throws an exception.
        You can expect this to propagate up to the calling code, which the test verifies. */

        var faultedTask = Task.FromException<Validation<int>>(new Exception("Simulated exception"));

        Func<Task> action = async () => { await faultedTask; };

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
    }
    
    [Fact]
    public async Task TestSelectMany_CollectionTaskThrowsException()
    {
        /* Similar to the previous test, except this time the exception originates from the subsequent operation.
        This test checks whether exceptions inside async operations are correctly propagated. */

        var sourceTask = Task.FromResult(Validation<int>.Valid(5));

        Func<Task> action = async () => await (from s in await sourceTask from c in FaultedSelector() select c + s);

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
    }
    
    [Fact]
    public void TestSelectMany_SyncBindingToInvalid()
    {
        /* This test simulates a situation where the source value is invalid. We can expect that no matter 
        what operation we try to apply, the result should also be invalid. */
        
        var source = Validation<int>.Invalid(UiNoTranslate("Simulated error"));

        var result = from s in source from res in Binder(s) select res;

        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(UiNoTranslate("Simulated error"));
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncBindingToInvalid()
    {
        /* The same scenario as the previous test, but with the source value provided by a Task. Again, 
        when the source is invalid, the result is expected to be invalid. */
        
        var sourceTask = Task.FromResult(Validation<int>.Invalid(UiNoTranslate("Simulated error")));

        var result = await (from s in await sourceTask from c in CollectionTaskSelector(s) select c + s);

        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(UiNoTranslate("Simulated error"));
    }

    [Fact]
    public async Task TestSelectMany_AsyncSourceInvalidDoesNotCallSelector()
    {
        /* This test ensures that the selector is not called when the source value is invalid. */
        
        var sourceTask = Task.FromResult(Validation<int>.Invalid(UiNoTranslate("Simulated error")));
        var selectorWasCalled = false;

        var result = await (from s in await sourceTask from c in CollectionTaskSelectorLocal(s) select c + s);
        
        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(UiNoTranslate("Simulated error"));
        selectorWasCalled.Should().BeFalse();
        
        return;

        async Task<Validation<int>> CollectionTaskSelectorLocal(int i)
        {
            selectorWasCalled = true;
            return await Task.FromResult(Validation<int>.Valid(i + 5));
        }
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps2()
    {
        /* This test demonstrates the binding of asynchronous operations in sequence.
         It starts with an asynchronous operation (sourceTask) and binds it to another asynchronous operation
         (collectionTaskSelector), resulting in a final transformation using the resultSelector function. */
        
        var sourceTask = Task.FromResult(Validation<int>.Valid(5));

        var result = await sourceTask.SelectMany(
            async s => await CollectionTaskSelector(s),
            (s, c) => s + c
        );

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(15);
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp2()
    {
        /* This test illustrates the binding of an asynchronous initial operation to a synchronous subsequent operation.
         It begins with an asynchronous operation (sourceTask) and binds it to a synchronous operation 
         (collectionSelector), resulting in a final transformation using the resultSelector function. */
        
        var sourceTask = Task.FromResult(Validation<int>.Valid(5));

        var result = await sourceTask.SelectMany(
            s => CollectionSelector(s),
            (s, c) => s + c
        );

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(11);
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps_SourceInvalid()
    {
        /* This test covers the scenario where the initial asynchronous operation returns an invalid result.
        Even though the operations are asynchronous, the final result should also be invalid. */
        
        var sourceTask = Task.FromResult(Validation<int>.Invalid(UiNoTranslate("Simulated error")));

        var result = await sourceTask.SelectMany(
            async s => await CollectionTaskSelector(s),
            (s, c) => s + c
        );

        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(UiNoTranslate("Simulated error"));
    }
    
    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_SourceInvalid()
    {
        /* This test examines the behavior when the initial asynchronous operation returns an invalid result.
        The subsequent synchronous operation should not be called, and the final result should be invalid. */
        
        var sourceTask = Task.FromResult(Validation<int>.Invalid(UiNoTranslate("Simulated error")));

        var result = await sourceTask.SelectMany(
            s => CollectionSelector(s),
            (s, c) => s + c
        );

        result.IsInvalid.Should().BeTrue();
        result.Errors.Should().Contain(UiNoTranslate("Simulated error"));
    }

    [Fact]
    public async Task TestSelectMany_AsyncBindingAsyncOps_Exception()
    {
        /* This test checks the behavior when the subsequent asynchronous operation throws an exception.
        The test ensures that the exception is correctly propagated. */
        
        var sourceTask = Task.FromResult(Validation<int>.Valid(5));

        Func<Task> action = async () => await sourceTask.SelectMany(
            async _ => await FaultedSelector(),
            (s, c) => s + c
        );

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
    }

    [Fact]
    public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_Exception()
    {
        /* This test ensures that when the synchronous subsequent operation throws an exception,
        it is correctly propagated, even if the initial operation was asynchronous. */
        
        var sourceTask = Task.FromResult(Validation<int>.Valid(5));

        Func<Task> action = async () => await sourceTask.SelectMany(
            _ => FaultedSelector(),
            (s, c) => s + c
        );

        await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
    }
    
    [Fact]
    public async Task TestSelectMany_ChainingMultipleOperations()
    {
        /* This test demonstrates chaining multiple operations together.
         It starts with an initial value and performs a series of synchronous and asynchronous operations. */
    
        var source = Validation<int>.Valid(5);

        var intermediateResult = await source
            .SelectMany(s => Task.FromResult(SyncOperation1(s)), (_, a) => a)
            .SelectMany(AsyncOperation2, (_, b) => b);

        var result = await intermediateResult
            .SelectMany(b => Task.FromResult(SyncOperation3(b)), (_, c) => c);

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(15);
    }

    static Validation<int> SyncOperation1(int i) => Validation<int>.Valid(i + 5);
    static async Task<Validation<int>> AsyncOperation2(int i) => await Task.FromResult(Validation<int>.Valid(i * 2));
    static Validation<int> SyncOperation3(int i) => Validation<int>.Valid(i - 5);
    
    [Fact]
    public async Task TestSelectMany_NestedValidationTypes()
    {
        /* This test ensures that nested Validation types are handled correctly.
         The source is a Validation<Validation<int>>, and the operations are performed on the inner value. */
        
        var source = Validation<Validation<int>>.Valid(Validation<int>.Valid(5));

        var result = await source.SelectMany(
            outer => Task.FromResult(outer),
            (_, inner) => inner + 5
        );

        result.IsValid.Should().BeTrue();
        result.Value.Should().Be(10);
    }
    
    [Fact]
    public async Task TestSelectMany_ComplexTypes()
    {
        /* This test demonstrates using more complex types instead of simple int values.
         It ensures that the Validation class works correctly with complex data types. */
        
        var source = Validation<Person>.Valid(new Person { Name = "Alice", Age = 30 });

        var result = await source.SelectMany(
            async p => await UpdateAgeAsync(p),
            (p, updatedAge) => new Person { Name = p.Name, Age = updatedAge }
        );

        result.IsValid.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new Person { Name = "Alice", Age = 35 });
    }

    static Validation<int> Binder(int i) => Validation<int>.Valid(i + 5);
    static Validation<int> CollectionSelector(int i) => Validation<int>.Valid(i + 1);
    static async Task<Validation<int>> CollectionTaskSelector(int i) => 
        await Task.FromResult(Validation<int>.Valid(i + 5));
    static async Task<Validation<int>> FaultedSelector() => 
        await Task.FromException<Validation<int>>(new Exception("Simulated exception"));

    static async Task<Validation<int>> UpdateAgeAsync(Person person)
    {
        return await Task.FromResult(Validation<int>.Valid(person.Age + 5));
    }
    private record Person
    {
        public string? Name { get; init; }
        public int Age { get; init; }
    }
}
