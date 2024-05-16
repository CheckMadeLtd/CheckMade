// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.FpExt.MonadicWrappers;

public record Result<T>
{
    internal T? Value { get; }
    internal bool Success { get; }
    internal string? Error { get; }

    // Implicit conversion from T to Result<T>
    public static implicit operator Result<T>(T value) => new(value);

    // Implicit conversion from string to Result<T> (for errors)
    public static implicit operator Result<T>(string error) => new(error);

    private Result(T value)
    {
        Value = value;
        Success = true;
    }

    private Result(string error)
    {
        Error = error;
        Success = false;
    }

    public static Result<T> FromSuccess(T value) => new Result<T>(value);
    public static Result<T> FromError(string error) => new Result<T>(error);
    
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError)
    {
        return Success ? onSuccess(Value!) : onError(Error!);
    }

    public T GetValueOrThrow()
    {
        if (Success)
        {
            return Value!;
        }
        throw new InvalidOperationException(Error);
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return Success ? Value! : defaultValue;
    }
    
    // Covers scenarios where you have a successful Result and want to bind it to another Result,
    // with both operations being synchronous.
    public Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> binder)
    {
        return Success ? binder(Value!) : Result<TResult>.FromError(Error!);
    }
    
    // Covers scenarios where you need to combine a successful Result with another Result to produce a final result,
    // all within synchronous operations.
    public Result<TResult> SelectMany<TCollection, TResult>(
        Func<T, Result<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!Success)
            return Result<TResult>.FromError(Error!);

        var collectionResult = collectionSelector(Value!);

        return collectionResult.Success
            ? Result<TResult>.FromSuccess(resultSelector(Value!, collectionResult.Value!))
            : Result<TResult>.FromError(collectionResult.Error!);
    }
}

public static class ResultExtensions
{
    // Covers scenarios where both the initial Result and the function it binds to are asynchronous operations,
    // allowing for the combination of their results asynchronously.
    public static async Task<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Result<TSource>> sourceTask,
        Func<TSource, Task<Result<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.Success)
            return Result<TResult>.FromError(source.Error!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.Success
            ? Result<TResult>.FromSuccess(resultSelector(source.Value!, collection.Value!))
            : Result<TResult>.FromError(collection.Error!);
    }

    // Covers scenarios where the initial Result is an asynchronous operation, but the function it binds to is
    // synchronous, enabling the combination of asynchronous and synchronous operations.
    public static async Task<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Result<TSource>> sourceTask,
        Func<TSource, Result<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.Success)
            return Result<TResult>.FromError(source.Error!);

        var collection = collectionSelector(source.Value!);

        return collection.Success
            ? Result<TResult>.FromSuccess(resultSelector(source.Value!, collection.Value!))
            : Result<TResult>.FromError(collection.Error!);
    }

    // Covers scenarios where you start with a synchronous Result<T>, but need to perform an asynchronous operation
    // based on the result.
    public static async Task<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Result<TSource> source,
        Func<TSource, Task<Result<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (!source.Success)
            return Result<TResult>.FromError(source.Error!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.Success
            ? Result<TResult>.FromSuccess(resultSelector(source.Value!, collection.Value!))
            : Result<TResult>.FromError(collection.Error!);
    }
}
