// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

public record Result<T>
{
    internal T? Value { get; }
    internal bool Success { get; }
    internal string? Error { get; }

    // Implicit conversion from T to Result<T>
    public static implicit operator Result<T>(T value) => new(value);

    // Implicit conversion from string to Result<T> (for errors)
    public static implicit operator Result<T>(string error) => new(error);

    public Result(T value)
    {
        Value = value;
        Success = true;
    }

    public Result(string error)
    {
        Error = error;
        Success = false;
    }

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
    
    public Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> binder)
    {
        return Success ? binder(Value!) : new Result<TResult>(Error!);
    }
    
    public Result<TResult> SelectMany<TCollection, TResult>(
        Func<T, Result<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!Success)
            return new Result<TResult>(Error!);

        var collectionResult = collectionSelector(Value!);

        return collectionResult.Success
            ? new Result<TResult>(resultSelector(Value!, collectionResult.Value!))
            : new Result<TResult>(collectionResult.Error!);
    }

}
