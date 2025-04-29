// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.LangExt.FpExtensions.Monads;

public sealed record Attempt<T>
{
    internal T? Value { get; }
    internal Exception? Exception { get; }
    
    public bool IsSuccess => Exception == null;
    public bool IsFailure => !IsSuccess;

    private Attempt(T value)
    {
        Value = value;
        Exception = null;
    }

    private Attempt(Exception exception)
    {
        Exception = exception;
        Value = default;
    }
    
    public static Attempt<T> Succeed(T value) => new(value);
    public static Attempt<T> Fail(Exception exception) => new(exception);
    
    public static implicit operator Attempt<T>(T value) => Succeed(value);  
    public static implicit operator Attempt<T>(Exception ex) => Fail(ex);
    
    public static Attempt<T> Run(Func<T> func)
    {
        try
        {
            return new Attempt<T>(func());
        }
        catch (Exception ex)
        {
            return new Attempt<T>(ex);
        }
    }
    
    public static async Task<Attempt<T>> RunAsync(Func<Task<T>> func)
    {
        try
        {
            return new Attempt<T>(await func());
        }
        catch (Exception ex) // also covers any AggregateException (e.g. from `await Task.WhenAll()`
        {
            return new Attempt<T>(ex);
        }
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Exception, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Exception!);
    }
    
    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? Value! : defaultValue;
    }

    public T GetValueOrThrow()
    {
        if (IsSuccess)
        {
            return Value!;
        }

        throw Exception!;
    }
}

public static class AttemptExtensions 
{
    public static Attempt<TResult> Select<T, TResult>(this Attempt<T> source, Func<T, TResult> selector)
    {
        return source.IsSuccess 
            ? selector(source.Value!) 
            : source.Exception!;
    }

    public static Attempt<T> Where<T>(this Attempt<T> source, Func<T, bool> predicate)
    {
        if (!source.IsSuccess) return source;

        return predicate(source.Value!) 
            ? source 
            : new Exception("Predicate not satisfied");
    }
    
    /* Covers scenarios where you have a successful attempt and want to bind it to another
    attempt, with both operations being synchronous.*/
    public static Attempt<TResult> SelectMany<T, TResult>(this Attempt<T> source, Func<T, Attempt<TResult>> binder)
    {
        return source.IsSuccess ? binder(source.Value!) : source.Exception!;
    }
    
    /* Covers scenarios where you need to combine a successful attempt with another attempt to
     produce a final result, all within synchronous operations. */
    public static Attempt<TResult> SelectMany<T, TCollection, TResult>(
        this Attempt<T> source,
        Func<T, Attempt<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!source.IsSuccess)
            return source.Exception!;
    
        var collectionAttempt = collectionSelector(source.Value!);

        return collectionAttempt.IsSuccess
            ? resultSelector(source.Value!, collectionAttempt.Value!)
            : collectionAttempt.Exception!;
    }
    
    /* Covers scenarios where both the initial attempt and the function it binds to are
     asynchronous operations, allowing for the combination of their results asynchronously */
    public static async Task<Attempt<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Attempt<TSource>> sourceTask,
        Func<TSource, Task<Attempt<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsSuccess)
            return source.Exception!;

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess 
            ? resultSelector(source.Value!, collection.Value!)
            : collection.Exception!;
    }
    
    /* Covers scenarios where the initial attempt is an asynchronous operation, but the function
     it binds to is synchronous, enabling the combination of asynchronous and synchronous operations */ 
    public static async Task<Attempt<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Attempt<TSource>> sourceTask,
        Func<TSource, Attempt<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsSuccess)
            return source.Exception!;

        var collection = collectionSelector(source.Value!);

        return collection.IsSuccess 
            ? resultSelector(source.Value!, collection.Value!)
            : collection.Exception!;
    }
    
    /* Handles cases where you start with a synchronous Attempt<T>, but need to perform an asynchronous operation based
     on the result */
    public static async Task<Attempt<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Attempt<TSource> source,
        Func<TSource, Task<Attempt<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (!source.IsSuccess)
            return source.Exception!;

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess
            ? resultSelector(source.Value!, collection.Value!)
            : collection.Exception!;
    }
}