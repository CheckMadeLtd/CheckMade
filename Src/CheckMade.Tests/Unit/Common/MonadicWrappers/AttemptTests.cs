// using System.Diagnostics;
// using FluentAssertions;
//
// namespace CheckMade.Tests.Unit.Common.Monads;
//
// public class AttemptTests
// {
//     [Fact]
//     public void TestAttempt_Match_Success()
//     {
//         var attempt = Attempt<int>.Succeed(5);
//         var result = attempt.Match(
//             onSuccess: value => value * 2,
//             onFailure: _ => 0);
//
//         result.Should().Be(10);
//     }
//
//     [Fact]
//     public void TestAttempt_Match_Failure()
//     {
//         var attempt = Attempt<int>.Fail(new Exception("Error"));
//         var result = attempt.Match(
//             onSuccess: value => value * 2,
//             onFailure: ex => ex.Message.Length);
//
//         result.Should().Be(5);
//     }
//
//     [Fact]
//     public void TestAttempt_GetValue_Success()
//     {
//         var attempt = Attempt<int>.Succeed(5);
//         var value = attempt.GetValueOrThrow();
//
//         value.Should().Be(5);
//     }
//
//     [Fact]
//     public void TestAttempt_GetValue_Failure()
//     {
//         var attempt = Attempt<int>.Fail(new Exception("Error"));
//
//         Action action = () => attempt.GetValueOrThrow();
//         action.Should().Throw<Exception>().WithMessage("Error");
//     }
//
//     [Fact]
//     public void TestAttempt_GetValueOrDefault_Success()
//     {
//         var attempt = Attempt<int>.Succeed(5);
//         var value = attempt.GetValueOrDefault(10);
//
//         value.Should().Be(5);
//     }
//
//     [Fact]
//     public void TestAttempt_GetValueOrDefault_Failure()
//     {
//         var attempt = Attempt<int>.Fail(new Exception("Error"));
//         var value = attempt.GetValueOrDefault(10);
//
//         value.Should().Be(10);
//     }
//     
//     [Fact]
//     public void Select_ShouldReturnSuccessfulResult_WhenSourceIsSuccessful()
//     {
//         // Arrange
//         var source = Attempt<int>.Succeed(2);
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
//     public void Select_ShouldReturnFailureResult_WhenSourceIsFailure()
//     {
//         // Arrange
//         var source = Attempt<int>.Fail(new Exception("Test exception"));
//         
//         // Act
//         var result = from s in source
//             select s * 2;
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         result.Exception!.Message.Should().Be("Test exception");
//     }
//     
//     [Fact]
//     public void Where_ShouldReturnSuccessfulResult_WhenSourceIsSuccessfulAndPredicateIsSatisfied()
//     {
//         // Arrange
//         var source = Attempt<int>.Succeed(2);
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
//         var source = Attempt<int>.Succeed(2);
//         
//         // Act
//         var result = from s in source
//             where s < 1
//             select s;
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         result.Exception!.Message.Should().Be("Predicate not satisfied");
//     }
//     
//     [Fact]
//     public void Where_ShouldReturnFailureResult_WhenSourceIsFailure()
//     {
//         // Arrange
//         var source = Attempt<int>.Fail(new Exception("Test exception"));
//         
//         // Act
//         var result = from s in source
//             where s > 1
//             select s;
//         
//         // Assert
//         result.IsSuccess.Should().BeFalse();
//         result.Exception!.Message.Should().Be("Test exception");
//     }
//     
//     [Fact]
//     public void TestSelectMany_SuccessToSuccess()
//     {
//         /* This test demonstrates binding a successful Attempt<T> to another successful Attempt<TResult>.
//          The initial value is successful, and the binder function also returns a successful Attempt<TResult>. */
//
//         var source = Attempt<int>.Succeed(5);
//         var result = source.SelectMany(Binder);
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(10);
//     }
//
//     [Fact]
//     public void TestSelectMany_SuccessToFailure()
//     {
//         /* This test demonstrates binding a successful Attempt<T> to a failed Attempt<TResult>.
//          The initial value is successful, but the binder function returns a failed Attempt<TResult>. */
//
//         var source = Attempt<int>.Succeed(5);
//         var result = source.SelectMany(BinderFail);
//
//         result.IsFailure.Should().BeTrue();
//         Trace.Assert(result.Exception != null, "result.Exception != null");
//         result.Exception.Message.Should().Be("Simulated error");
//     }
//
//     [Fact]
//     public void TestSelectMany_FailureToBinding()
//     {
//         /* This test demonstrates binding a failed Attempt<T> to any Attempt<TResult>.
//          The initial value is a failure, and we expect the failure to propagate without calling the binder function. */
//
//         var source = Attempt<int>.Fail(new Exception("Initial error"));
//         var result = source.SelectMany(Binder);
//
//         result.IsFailure.Should().BeTrue();
//         Trace.Assert(result.Exception != null, "result.Exception != null");
//         result.Exception.Message.Should().Be("Initial error");
//     }
//
//     static Attempt<int> BinderFail(int i) => Attempt<int>.Fail(new Exception("Simulated error"));
//     
//     [Fact]
//     public void TestSelectMany_SyncBindingSyncOps()
//     {
//         /* This test demonstrates synchronous binding of synchronous operations. It starts from an initial synchronous
//          value (retrieved from the source variable), and then pipeline it to another synchronous operation such as 
//          mapping, filtering, or flatmapping. 
//          The key feature here is that all operations are done in a synchronous manner. */
//         
//         var source = Attempt<int>.Succeed(5);
//         var result = from s in source from res in Binder(s) select res;
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(10);
//     }
//     
//     [Fact]
//     public void TestSelectMany_CombineTwoSyncOps()
//     {
//         /* This test shows how to combine two synchronous operations to produce a final result.
//          Here we have an additional function (collectionSelector) which operates on the source value and produces
//          another Attempt<T>. The final result is obtained by combining the source and selected values. */
//         
//         var source = Attempt<int>.Succeed(5);
//
//         var result = 
//             from s in source 
//             from c in ConditionalCollectionSelector(s)
//             select s + c;
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
//         var sourceTask = Task.FromResult(Attempt<int>.Succeed(5));
//         var result = await (from s in await sourceTask from c in CollectionTaskSelector(s) select c + s);
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
//         var sourceTask = Task.FromResult(Attempt<int>.Succeed(5));
//         var result = from s in await sourceTask from c in CollectionSelector(s) select c + s;
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
//         var source = Attempt<int>.Succeed(5);
//         var result = await (from s in source from c in CollectionTaskSelector(s) select c + s);
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
//         Task<Attempt<int>> faultedTask = Task.FromException<Attempt<int>>(new Exception("Simulated exception"));
//         Func<Task> action = async () => { await faultedTask; };
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_CollectionTaskThrowsException()
//     {
//         /* Similar to the previous test, except this time the exception originates from the subsequent operation.
//         This test checks whether exceptions inside async operations are correctly propagated. */
//
//         var sourceTask = Task.FromResult(Attempt<int>.Succeed(5));
//         Func<Task> action = async () => await (from s in await sourceTask from c in FaultedSelector() select c + s);
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//     
//     [Fact]
//     public void TestSelectMany_SyncBindingToFail()
//     {
//         /* This test simulates a situation where the source value is a failure. We can expect that no matter 
//         what operation we try to apply, the result should also be a failure. */
//         
//         var source = Attempt<int>.Fail(new Exception("Simulated exception"));
//         var result = from s in source from res in Binder(s) select res;
//         result.IsFailure.Should().BeTrue();
//         result.Exception.Should().BeOfType<Exception>();
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingToFail()
//     {
//         /* The same scenario as the previous test, but with the source value provided by a Task. Again, 
//         when the source is a failure, the result is expected to be a failure. */
//         
//         var sourceTask = Task.FromResult(Attempt<int>.Fail(new Exception("Simulated exception")));
//         var result = await (from s in await sourceTask from c in CollectionTaskSelector(s) select c + s);
//         result.IsFailure.Should().BeTrue();
//         result.Exception.Should().BeOfType<Exception>();
//     }
//
//     [Fact]
//     public async Task TestSelectMany_AsyncSourceNoneDoesNotCallSelector()
//     {
//         /* This test ensures that the selector is not called when the source value is a failure. */
//         
//         var sourceTask = Task.FromResult(Attempt<int>.Fail(new Exception("Simulated exception")));
//         var selectorWasCalled = false;
//
//         var result = await (from s in await sourceTask from c in CollectionTaskSelectorLocal(s) select c + s);
//         
//         result.IsFailure.Should().BeTrue();
//         result.Exception.Should().BeOfType<Exception>();
//         selectorWasCalled.Should().BeFalse();
//         
//         return;
//
//         async Task<Attempt<int>> CollectionTaskSelectorLocal(int i)
//         {
//             selectorWasCalled = true;
//             return await Task.FromResult(Attempt<int>.Succeed(i + 5));
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
//         var sourceTask = Task.FromResult(Attempt<int>.Succeed(5));
//         var result = await sourceTask.SelectMany(
//             async s => await CollectionTaskSelector(s),
//             (s, c) => s + c
//         );
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
//         var sourceTask = Task.FromResult(Attempt<int>.Succeed(5));
//         var result = await sourceTask.SelectMany(
//             s => CollectionSelector(s),
//             (s, c) => s + c
//         );
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(11);
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingAsyncOps_SourceNone()
//     {
//         /* This test covers the scenario where the initial asynchronous operation returns a failure.
//         Even though the operations are asynchronous, the final result should also be a failure. */
//         
//         var sourceTask = Task.FromResult(Attempt<int>.Fail(new Exception("Simulated exception")));
//         var result = await sourceTask.SelectMany(
//             async s => await CollectionTaskSelector(s),
//             (s, c) => s + c
//         );
//         result.IsFailure.Should().BeTrue();
//         result.Exception.Should().BeOfType<Exception>();
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_SourceNone()
//     {
//         /* This test examines the behavior when the initial asynchronous operation returns a failure.
//         The subsequent synchronous operation should not be called, and the final result should be a failure. */
//         
//         var sourceTask = Task.FromResult(Attempt<int>.Fail(new Exception("Simulated exception")));
//         var result = await sourceTask.SelectMany(
//             s => CollectionSelector(s),
//             (s, c) => s + c
//         );
//         result.IsFailure.Should().BeTrue();
//         result.Exception.Should().BeOfType<Exception>();
//     }
//
//     [Fact]
//     public async Task TestSelectMany_AsyncBindingAsyncOps_Exception()
//     {
//         /* This test checks the behavior when the subsequent asynchronous operation throws an exception.
//         The test ensures that the exception is correctly propagated. */
//         
//         var sourceTask = Task.FromResult(Attempt<int>.Succeed(5));
//         Func<Task> action = async () => await sourceTask.SelectMany(
//             async _ => await FaultedSelector(),
//             (s, c) => s + c
//         );
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//
//     [Fact]
//     public async Task TestSelectMany_AsyncInitOpBindingToSyncSubsequentOp_Exception()
//     {
//         /* This test ensures that when the synchronous subsequent operation throws an exception,
//         it is correctly propagated, even if the initial operation was asynchronous. */
//         
//         var sourceTask = Task.FromResult(Attempt<int>.Succeed(5));
//         Func<Task> action = async () => await sourceTask.SelectMany(
//             _ => FaultedSelector(),
//             (s, c) => s + c
//         );
//         await action.Should().ThrowAsync<Exception>().WithMessage("Simulated exception");
//     }
//     
//     [Fact]
//     public async Task TestSelectMany_ChainingMultipleOperations()
//     {
//         /* This test demonstrates chaining multiple operations together.
//          It starts with an initial value and performs a series of synchronous and asynchronous operations. */
//     
//         var source = Attempt<int>.Succeed(5);
//
//         var result = await source
//             .SelectMany(s => Task.FromResult(SyncOperation1(s)), (_, a) => a)
//             .SelectMany(a => AsyncOperation2(a), (_, b) => b)
//             .SelectMany(b => Task.FromResult(SyncOperation3(b)), (_, c) => c);
//
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().Be(15); // Expected value corrected to 15
//     }
//
//     static Attempt<int> SyncOperation1(int i) => Attempt<int>.Succeed(i + 5);
//     static async Task<Attempt<int>> AsyncOperation2(int i) => await Task.FromResult(Attempt<int>.Succeed(i * 2));
//     static Attempt<int> SyncOperation3(int i) => Attempt<int>.Succeed(i - 5);
//
//     [Fact]
//     public async Task TestSelectMany_NestedAttemptTypes()
//     {
//         /* This test ensures that nested Attempt types are handled correctly.
//          The source is an Attempt<Attempt<int>>, and the operations are performed on the inner value. */
//
//         var source = Attempt<Attempt<int>>.Succeed(Attempt<int>.Succeed(5));
//     
//         var result = await source.SelectMany(
//             outer => Task.FromResult(outer),
//             (_, inner) => inner + 5 // Directly add 5 to the inner value
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
//          It ensures that the Attempt class works correctly with complex data types. */
//         
//         var source = Attempt<Person>.Succeed(new Person { Name = "Alice", Age = 30 });
//         var result = await source.SelectMany(
//             async p => await UpdateAgeAsync(p),
//             (p, updatedAge) => new Person { Name = p.Name, Age = updatedAge }
//         );
//         result.IsSuccess.Should().BeTrue();
//         result.Value.Should().BeEquivalentTo(new Person { Name = "Alice", Age = 35 });
//     }
//
//     static Attempt<int> Binder(int i) => Attempt<int>.Succeed(i + 5);
//     static Attempt<int> CollectionSelector(int i) => Attempt<int>.Succeed(i + 1);
//     static async Task<Attempt<int>> CollectionTaskSelector(int i) => await Task.FromResult(Attempt<int>.Succeed(i + 5));
//     static async Task<Attempt<int>> FaultedSelector() => await Task.FromException<Attempt<int>>(new Exception("Simulated exception"));
//     static Attempt<int> ConditionalCollectionSelector(int i) 
//     {
//         int value = i + 1;
//         return value > 5 ? Attempt<int>.Succeed(value) : Attempt<int>.Fail(new Exception("Not matched"));
//     }
//     static async Task<Attempt<int>> UpdateAgeAsync(Person person)
//     {
//         return await Task.FromResult(Attempt<int>.Succeed(person.Age + 5));
//     }
//     private record Person
//     {
//         public string? Name { get; init; }
//         public int Age { get; init; }
//     }
// }
