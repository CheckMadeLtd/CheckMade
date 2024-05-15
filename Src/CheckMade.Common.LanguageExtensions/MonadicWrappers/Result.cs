// ReSharper disable MemberCanBePrivate.Global
namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

public record Result<T>
{
    private readonly T? _value;
    private readonly bool _success;
    private readonly string? _error;

    // Implicit conversion from T to Result<T>
    public static implicit operator Result<T>(T value) => new(value);

    // Implicit conversion from string to Result<T> (for errors)
    public static implicit operator Result<T>(string error) => new(error);

    public Result(T value)
    {
        _value = value;
        _success = true;
    }

    public Result(string error)
    {
        _error = error;
        _success = false;
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError)
    {
        return _success ? onSuccess(_value!) : onError(_error!);
    }

    public T GetValueOrThrow()
    {
        if (_success)
        {
            return _value!;
        }
        throw new InvalidOperationException(_error);
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return _success ? _value! : defaultValue;
    }
    
    public Result<TResult> SelectMany<TResult>(Func<T, Result<TResult>> binder)
    {
        return _success ? binder(_value!) : new Result<TResult>(_error!);
    }
}
