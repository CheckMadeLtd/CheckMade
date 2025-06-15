namespace CheckMade.Common.Utils.FpExtensions.Monads;

public static class ResultExtensions
{
    #region Basic Transformations
    
    public static Result<TResult> Select<T, TResult>(this Result<T> source, Func<T, TResult> selector)
    {
        return source.IsSuccess
            ? Result<TResult>.Succeed(selector(source.Value!))
            : Result<TResult>.Fail(source.FailureInfo!);
    }

    public static Result<T> Where<T>(this Result<T> source, Func<T, bool> predicate)
    {
        if (!source.IsSuccess) return source;

        return predicate(source.Value!) 
            ? source 
            : Result<T>.Fail(UiNoTranslate("Predicate not satisfied"));
    }

    #endregion
    
    #region Synchronous SelectMany (Bind) Operations
    
    // Basic synchronous binding
    // For use with LINQ fluent syntax
    public static Result<TResult> SelectMany<T, TResult>(
        this Result<T> source, 
        Func<T, Result<TResult>> binder)
    {
        return source.IsSuccess ? binder(source.Value!) : Result<TResult>.Fail(source.FailureInfo!);
    }
    
    // Combining two synchronous operations to produce a final result
    // For use with LINQ query syntax
    // collectionSelector is the function that takes the result of the previous operation and returns the next monadic operation
    // resultSelector is the function that combines values from both operations
    // (the resultSelector is keeping track of all previous values so they're available for the final select clause)
    public static Result<TResult> SelectMany<T, TCollection, TResult>(
        this Result<T> source,
        Func<T, Result<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!source.IsSuccess)
            return Result<TResult>.Fail(source.FailureInfo!);

        var collectionResult = collectionSelector(source.Value!);

        return collectionResult.IsSuccess
            ? Result<TResult>.Succeed(resultSelector(source.Value!, collectionResult.Value!))
            : Result<TResult>.Fail(collectionResult.FailureInfo!);
    }
    
    #endregion
    
    #region Task-based SelectMany Operations

    // Binding asynchronous operations with asynchronous source
    public static async Task<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Result<TSource>> sourceTask,
        Func<TSource, Task<Result<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsSuccess)
            return Result<TResult>.Fail(source.FailureInfo!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess
            ? Result<TResult>.Succeed(resultSelector(source.Value!, collection.Value!))
            : Result<TResult>.Fail(collection.FailureInfo!);
    }

    // Binding synchronous operations with asynchronous source
    public static async Task<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Result<TSource>> sourceTask,
        Func<TSource, Result<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsSuccess)
            return Result<TResult>.Fail(source.FailureInfo!);

        var collection = collectionSelector(source.Value!);

        return collection.IsSuccess
            ? Result<TResult>.Succeed(resultSelector(source.Value!, collection.Value!))
            : Result<TResult>.Fail(collection.FailureInfo!);
    }

    // Binding asynchronous operations with synchronous source
    public static async Task<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Result<TSource> source,
        Func<TSource, Task<Result<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (!source.IsSuccess)
            return Result<TResult>.Fail(source.FailureInfo!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess
            ? Result<TResult>.Succeed(resultSelector(source.Value!, collection.Value!))
            : Result<TResult>.Fail(collection.FailureInfo!);
    }

    #endregion
}