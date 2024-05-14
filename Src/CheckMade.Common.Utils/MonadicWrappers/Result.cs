// ReSharper disable MemberCanBePrivate.Global
namespace CheckMade.Common.Utils.MonadicWrappers;

public record Result<T>
{
    private readonly T? _value;
    private readonly bool _success;
    private readonly string? _error;

    // Implicit conversion from T to Result<T>
    public static implicit operator Result<T>(T value) => new Result<T>(value);

    // Implicit conversion from string to Result<T> (for errors)
    public static implicit operator Result<T>(string error) => new Result<T>(error);

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
}
