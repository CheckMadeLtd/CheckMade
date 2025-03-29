// using FluentAssertions;
//
// namespace CheckMade.Tests.Unit.Common.MonadicWrappers;
//
// public sealed class ResultTests
// {
//     [Fact]
//     public void TestResult_Match_Success()
//     {
//         var result = Result<int>.FromSuccess(5);
//         var output = result.Match(
//             onSuccess: value => value * 2,
//             onError: _ => 0);
//
//         output.Should().Be(10);
//     }
//
//     [Fact]
//     public void TestResult_Match_Error()
//     {
//         var result = Result<int>.FromError(UiNoTranslate("Error"));
//         var output = result.Match(
//             onSuccess: value => value * 2,
//             onError: error => error.GetFormattedEnglish().Length);
//
//         output.Should().Be(5);
//     }
//
//     [Fact]
//     public void TestResult_GetValue_Success()
//     {
//         var result = Result<int>.FromSuccess(5);
//         var value = result.GetValueOrThrow();
//
//         value.Should().Be(5);
//     }
//
//     [Fact]
//     public void TestResult_GetValue_Error()
//     {
//         var result = Result<int>.FromError(UiNoTranslate("Error"));
//
//         Action action = () => result.GetValueOrThrow();
//         action.Should().Throw<InvalidOperationException>().WithMessage("Error");
//     }
//
//     [Fact]
//     public void TestResult_GetValueOrDefault_Success()
//     {
//         var result = Result<int>.FromSuccess(5);
//         var value = result.GetValueOrDefault(10);
//
//         value.Should().Be(5);
//     }
//
//     [Fact]
//     public void TestResult_GetValueOrDefault_Error()
//     {
//         var result = Result<int>.FromError(UiNoTranslate("Error"));
//         var value = result.GetValueOrDefault(10);
//
//         value.Should().Be(10);
//     }
//     
//     [Fact]
//     public void Select_ShouldReturnSuccessfulResult_WhenSourceIsSuccessful()
//     {
//         // Arrange
//         var source = Result<int>.FromSuccess(2);
//         
//         // Act
//         var result = from s in source
//             select s * 2;
//         
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(4);
//     }
//     
//     [Fact]
//     public void Select_ShouldReturnFailureResult_WhenSourceIsError()
//     {
//         // Arrange
//         var source = Result<int>.FromError(UiNoTranslate("Error message"));
//         
//         // Act
//         var result = from s in source
//             select s * 2;
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Error message");
//     }
//     
//     [Fact]
//     public void Where_ShouldReturnSuccessfulResult_WhenSourceIsSuccessfulAndPredicateIsSatisfied()
//     {
//         // Arrange
//         var source = Result<int>.FromSuccess(2);
//         
//         // Act
//         var result = from s in source
//             where s > 1
//             select s;
//         
//         // Assert
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(2);
//     }
//     
//     [Fact]
//     public void Where_ShouldReturnFailureResult_WhenSourceIsSuccessfulAndPredicateIsNotSatisfied()
//     {
//         // Arrange
//         var source = Result<int>.FromSuccess(2);
//         
//         // Act
//         var result = from s in source
//             where s < 1
//             select s;
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Predicate not satisfied");
//     }
//     
//     [Fact]
//     public void Where_ShouldReturnFailureResult_WhenSourceIsError()
//     {
//         // Arrange
//         var source = Result<int>.FromError(UiNoTranslate("Error message"));
//         
//         // Act
//         var result = from s in source
//             where s > 1
//             select s;
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Error message");
//     }
//     
//     [Fact]
//     public void TestSelectMany_SuccessToSuccess()
//     {
//         /* This test demonstrates binding a successful Result<T> to another successful Result<TResult>.
//          The initial value is successful, and the binder function also returns a successful Result<TResult>. */
//
//         var source = Result<int>.FromSuccess(5);
//         var result = source.SelectMany(Binder);
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(10);
//     }
//
//     [Fact]
//     public void TestSelectMany_SuccessToError()
//     {
//         /* This test demonstrates binding a successful Result<T> to an erroneous Result<TResult>.
//          The initial value is successful, but the binder function returns an erroneous Result<TResult>. */
//
//         var source = Result<int>.FromSuccess(5);
//         var result = source.SelectMany(BinderError);
//
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Simulated error");
//
//         static Result<int> BinderError(int i) => Result<int>.FromError(UiNoTranslate("Simulated error"));
//     }
//
//     [Fact]
//     public void TestSelectMany_ErrorToBinding()
//     {
//         /* This test demonstrates binding an erroneous Result<T> to any Result<TResult>.
//          The initial value is an error, and we expect the error to propagate without calling the binder function. */
//
//         var source = Result<int>.FromError(UiNoTranslate("Initial error"));
//         var result = source.SelectMany(Binder);
//
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Initial error");
//     }
//     
//     [Fact]
//     public void TestSelectMany_SyncBindingSyncOps()
//     {
//         /* This test demonstrates synchronous binding of synchronous operations. It starts from an initial synchronous
//          value (retrieved from the source variable), and then pipelines it to another synchronous operation such as 
//          mapping, filtering, or flatmapping. 
//          The key feature here is that all operations are done in a synchronous manner. */
//         
//         var source = Result<int>.FromSuccess(5);
//
//         var result = from s in source from res in Binder(s) select res;
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(10);
//     }
//     
//     [Fact]
//     public void TestSelectMany_CombineTwoSyncOps()
//     {
//         /* This test shows how to combine two synchronous operations to produce a final result.
//          Here we have an additional function (collectionSelector) which operates on the source value and produces
//          another Result<T>. The final result is obtained by combining the source and selected values. */
//         
//         var source = Result<int>.FromSuccess(5);
//
//         var result = from s in source from c in CollectionSelector(s) select s + c;
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(11);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingAsyncOps()
//     {
//         /* This test illustrates asynchronous binding of asynchronous operations.
//          It starts with an asynchronous operation (sourceTask). Once the task resolves, the result is piped into 
//          another asynchronous operation (collectionTaskSelector). 
//          The await keyword is used to allow asynchronous execution of tasks. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromSuccess(5));
//
//         var result = await (from s in await sourceTask from c in CollectionTaskSelector(s) select c + s);
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(15);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp()
//     {
//         /* This unit test details the binding of an asynchronous initial operation to a synchronous subsequent
//          operation. Initially, an asynchronous operation is called (sourceTask), and once this completes, its result 
//          is passed to a synchronous operation (collectionSelector). 
//          This example shows how to efficiently chain asynchronous and synchronous operations. */ 
//         
//         var sourceTask = Task.FromResult(Result<int>.FromSuccess(5));
//
//         var result = from s in await sourceTask from c in CollectionSelector(s) select c + s;
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(11);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_SyncInitOpBindingToAsyncSubsequentOp()
//     {
//         /* In this test, a synchronous operation initiates and binds to an asynchronous subsequent operation.
//          A synchronous operation (source) is executed first, and its result is used in an asynchronous function 
//          (collectionTaskSelector) to yield the final result. */
//         
//         var source = Result<int>.FromSuccess(5);
//
//         var result = await (from s in source from c in CollectionTaskSelector(s) select c + s);
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(15);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_SourceTaskThrowsException()
//     {
//         /* This test shows what happens when the initial operation—the source Task—throws an exception.
//         You can expect this to propagate up to the calling code, which the test verifies. */
//
//         Task<Result<int>> faultedTask = Task.FromException<Result<int>>(new Exception("Simulated exception"));
//
//         Func<Task> action = async () => { await faultedTask; };
//
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_CollectionTaskThrowsException()
//     {
//         /* Similar to the previous test, except this time the exception originates from the subsequent operation.
//         This test checks whether exceptions inside async operations are correctly propagated. */
//
//         var sourceTask = Task.FromResult(Result<int>.FromSuccess(5));
//
//         Func<Task> action = async () => await (from s in await sourceTask from c in FaultedSelector() select c + s);
//
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//     
//     [Fact]
//     public void TestSelectMany_SyncBindingToError()
//     {
//         /* This test simulates a situation where the source value is an error. We can expect that no matter 
//         what operation we try to apply, the result should also be an error. */
//         
//         var source = Result<int>.FromError(UiNoTranslate("Simulated error"));
//
//         var result = from s in source from res in Binder(s) select res;
//
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Simulated error");
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingToError()
//     {
//         /* The same scenario as the previous test, but with the source value provided by a Task. Again, 
//         when the source is an error, the result is expected to be an error. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromError(UiNoTranslate("Simulated error")));
//
//         var result = await (from s in await sourceTask from c in CollectionTaskSelector(s) select c + s);
//
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Simulated error");
//     }
//
//     [Fact]
//     public async Task TestSelectMany_AsyncSourceErrorDoesNotCallSelector()
//     {
//         /* This test ensures that the selector is not called when the source value is an error. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromError(UiNoTranslate("Simulated error")));
//         var selectorWasCalled = false;
//
//         var result = await (from s in await sourceTask from c in CollectionTaskSelectorLocal(s) select c + s);
//         
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Simulated error");
//         selectorWasCalled.Should().BeFalse();
//         
//         return;
//
//         async Task<Result<int>> CollectionTaskSelectorLocal(int i)
//         {
//             selectorWasCalled = true;
//             return await Task.FromResult(Result<int>.FromSuccess(i + 5));
//         }
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingAsyncOps2()
//     {
//         /* This test demonstrates the binding of asynchronous operations in sequence.
//          It starts with an asynchronous operation (sourceTask) and binds it to another asynchronous operation
//          (collectionTaskSelector), resulting in a final transformation using the resultSelector function. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromSuccess(5));
//
//         var result = await sourceTask.SelectMany(
//             async s => await CollectionTaskSelector(s),
//             (s, c) => s + c
//         );
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(15);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp2()
//     {
//         /* This test illustrates the binding of an asynchronous initial operation to a synchronous subsequent operation.
//          It begins with an asynchronous operation (sourceTask) and binds it to a synchronous operation 
//          (collectionSelector), resulting in a final transformation using the resultSelector function. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromSuccess(5));
//
//         var result = await sourceTask.SelectMany(
//             s => CollectionSelector(s),
//             (s, c) => s + c
//         );
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(11);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingAsyncOps_SourceError()
//     {
//         /* This test covers the scenario where the initial asynchronous operation returns an error.
//         Even though the operations are asynchronous, the final result should also be an error. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromError(UiNoTranslate("Simulated error")));
//
//         var result = await sourceTask.SelectMany(
//             async s => await CollectionTaskSelector(s),
//             (s, c) => s + c
//         );
//
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Simulated error");
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_SourceError()
//     {
//         /* This test examines the behavior when the initial asynchronous operation returns an error.
//         The subsequent synchronous operation should not be called, and the final result should be an error. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromError(UiNoTranslate("Simulated error")));
//
//         var result = await sourceTask.SelectMany(
//             s => CollectionSelector(s),
//             (s, c) => s + c
//         );
//
//         result.IsSuccess.Should().BeFalse();
//         result.Error!.GetFormattedEnglish().Should().Be("Simulated error");
//     }
//
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingAsyncOps_Exception()
//     {
//         /* This test checks the behavior when the subsequent asynchronous operation throws an exception.
//         The test ensures that the exception is correctly propagated. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromSuccess(5));
//
//         Func<Task> action = async () => await sourceTask.SelectMany(
//             async _ => await FaultedSelector(),
//             (s, c) => s + c
//         );
//
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//
//     [Fact]
//     public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_Exception()
//     {
//         /* This test ensures that when the synchronous subsequent operation throws an exception,
//         it is correctly propagated, even if the initial operation was asynchronous. */
//         
//         var sourceTask = Task.FromResult(Result<int>.FromSuccess(5));
//
//         Func<Task> action = async () => await sourceTask.SelectMany(
//             _ => FaultedSelector(),
//             (s, c) => s + c
//         );
//
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_ChainingMultipleOperations()
//     {
//         /* This test demonstrates chaining multiple operations together.
//          It starts with an initial value and performs a series of synchronous and asynchronous operations. */
//     
//         var source = Result<int>.FromSuccess(5);
//
//         var intermediateResult = await source
//             .SelectMany(s => Task.FromResult(SyncOperation1(s)), (_, a) => a)
//             .SelectMany(AsyncOperation2, (_, b) => b);
//
//         var result = await intermediateResult
//             .SelectMany(b => Task.FromResult(SyncOperation3(b)), (_, c) => c);
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(15);
//     }
//
//     static Result<int> SyncOperation1(int i) => Result<int>.FromSuccess(i + 5);
//     static async Task<Result<int>> AsyncOperation2(int i) => await Task.FromResult(Result<int>.FromSuccess(i * 2));
//     static Result<int> SyncOperation3(int i) => Result<int>.FromSuccess(i - 5);
//     
//     [Fact]
//     public async Task TestSelectMany_NestedResultTypes()
//     {
//         /* This test ensures that nested Result types are handled correctly.
//          The source is a Result<Result<int>>, and the operations are performed on the inner value. */
//         
//         var source = Result<Result<int>>.FromSuccess(Result<int>.FromSuccess(5));
//
//         var result = await source.SelectMany(
//             outer => Task.FromResult(outer),
//             (_, inner) => inner + 5
//         );
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(10);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_ComplexTypes()
//     {
//         /* This test demonstrates using more complex types instead of simple int values.
//          It ensures that the Result class works correctly with complex data types. */
//         
//         var source = Result<Person>.FromSuccess(new Person { Name = "Alice", Age = 30 });
//
//         var result = await source.SelectMany(
//             async p => await UpdateAgeAsync(p),
//             (p, updatedAge) => new Person { Name = p.Name, Age = updatedAge }
//         );
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().BeEquivalentTo(new Person { Name = "Alice", Age = 35 });
//     }
//
//     static Result<int> Binder(int i) => Result<int>.FromSuccess(i + 5);
//     static Result<int> CollectionSelector(int i) => Result<int>.FromSuccess(i + 1);
//     static async Task<Result<int>> CollectionTaskSelector(int i) => await Task.FromResult(Result<int>.FromSuccess(i + 5));
//     static async Task<Result<int>> FaultedSelector() => await Task.FromException<Result<int>>(new Exception("Simulated exception"));
//
//     static async Task<Result<int>> UpdateAgeAsync(Person person)
//     {
//         return await Task.FromResult(Result<int>.FromSuccess(person.Age + 5));
//     }
//     private record Person
//     {
//         public string? Name { get; init; }
//         public int Age { get; init; }
//     }
// }
