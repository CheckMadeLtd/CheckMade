// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.LangExt.MonadicWrappers;

public sealed record Result<T>
{
    internal T? Value { get; }
    internal UiString? Error { get; }

    public bool IsSuccess => Error == null;
    public bool IsError => !IsSuccess;

    private Result(T value)
    {
        Value = value;
    }

    private Result(UiString error)
    {
        Error = error;
    }

    public static Result<T> FromSuccess(T value) => new(value);
    public static Result<T> FromError(UiString error) => new(error);
    
    public static implicit operator Result<T>(T value) => FromSuccess(value);  
    public static implicit operator Result<T>(UiString error) => FromError(error);
    
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<UiString, TResult> onError)
    {
        return IsSuccess ? onSuccess(Value!) : onError(Error!);
    }

    public T GetValueOrThrow()
    {
        if (IsSuccess)
        {
            return Value!;
        }

        throw new InvalidOperationException(Error?.GetFormattedEnglish());
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? Value! : defaultValue;
    }
}

public static class ResultExtensions
{
    public static Result<TResult> Select<T, TResult>(this Result<T> source, Func<T, TResult> selector)
    {
        return source.IsSuccess 
            ? Result<TResult>.FromSuccess(selector(source.Value!)) 
            : Result<TResult>.FromError(source.Error!);
    }

    public static Result<T> Where<T>(this Result<T> source, Func<T, bool> predicate)
    {
        if (!source.IsSuccess) return source;

        return predicate(source.Value!) 
            ? source 
            : Result<T>.FromError(UiNoTranslate("Predicate not satisfied"));
    }
    
    // Covers scenarios where you have a successful Result and want to bind it to another Result,
    // with both operations being synchronous.
    public static Result<TResult> SelectMany<T, TResult>(
        this Result<T> source,
        Func<T, Result<TResult>> binder)
    {
        return source.IsSuccess ? binder(source.Value!) : Result<TResult>.FromError(source.Error!);
    }
    
    // Covers scenarios where you need to combine a successful Result with another Result to produce a final result,
    // all within synchronous operations.
    public static Result<TResult> SelectMany<T, TCollection, TResult>(
        this Result<T> source,
        Func<T, Result<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!source.IsSuccess)
            return Result<TResult>.FromError(source.Error!);

        var collectionResult = collectionSelector(source.Value!);

        return collectionResult.IsSuccess
            ? Result<TResult>.FromSuccess(resultSelector(source.Value!, collectionResult.Value!))
            : Result<TResult>.FromError(collectionResult.Error!);
    }
    
    // Covers scenarios where both the initial Result and the function it binds to are asynchronous operations,
    // allowing for the combination of their results asynchronously.
    public static async Task<Result<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Result<TSource>> sourceTask,
        Func<TSource, Task<Result<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsSuccess)
            return Result<TResult>.FromError(source.Error!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess
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

        if (!source.IsSuccess)
            return Result<TResult>.FromError(source.Error!);

        var collection = collectionSelector(source.Value!);

        return collection.IsSuccess
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
        if (!source.IsSuccess)
            return Result<TResult>.FromError(source.Error!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess
            ? Result<TResult>.FromSuccess(resultSelector(source.Value!, collection.Value!))
            : Result<TResult>.FromError(collection.Error!);
    }
}
