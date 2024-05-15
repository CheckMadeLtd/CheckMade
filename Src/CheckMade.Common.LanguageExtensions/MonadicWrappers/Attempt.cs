// ReSharper disable MemberCanBePrivate.Global
namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

public record Attempt<T>
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

    internal static Attempt<T> FromValue(T value) => new (value);
    internal static Attempt<T> FromException(Exception exception) => new (exception);
    
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
        catch (Exception ex)
        {
            return new Attempt<T>(ex);
        }
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Exception, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Exception!);
    }

    public static Attempt<Unit> Succeed()
    {
        return new Attempt<Unit>(Unit.Value);
    }
    
    public static Attempt<T> Fail(Exception exception)
    {
        return new Attempt<T>(exception);
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
    
    /* Covers scenarios where you have a successful attempt and want to bind it to another
     attempt, with both operations being synchronous.*/
    public Attempt<TResult> SelectMany<TResult>(Func<T, Attempt<TResult>> binder)
    {
        return IsSuccess ? binder(Value!) : new Attempt<TResult>(Exception!);
    }
    
    /* Covers scenarios where you need to combine a successful attempt with another attempt to
     produce a final result, all within synchronous operations. */
    public Attempt<TResult> SelectMany<TCollection, TResult>(
        Func<T, Attempt<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!IsSuccess)
            return new Attempt<TResult>(Exception!);
    
        var collectionAttempt = collectionSelector(Value!);

        return collectionAttempt.IsSuccess
            ? new Attempt<TResult>(resultSelector(Value!, collectionAttempt.Value!))
            : new Attempt<TResult>(collectionAttempt.Exception!);
    }
}
