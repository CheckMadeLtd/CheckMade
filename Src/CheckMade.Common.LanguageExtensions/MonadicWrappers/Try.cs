// ReSharper disable MemberCanBePrivate.Global
namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

public record Try<T>
{
    private readonly T? _value;
    private readonly Exception? _exception;
    
    public bool IsSuccess => _exception == null;
    public bool IsFailure => !IsSuccess;

    private Try(T value)
    {
        _value = value;
        _exception = null;
    }

    private Try(Exception exception)
    {
        _exception = exception;
        _value = default;
    }

    public static Try<T> Run(Func<T> func)
    {
        try
        {
            return new Try<T>(func());
        }
        catch (Exception ex)
        {
            return new Try<T>(ex);
        }
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Exception, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(_exception!);
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSuccess ? _value! : defaultValue;
    }

    public T GetValueOrThrow()
    {
        if (IsSuccess)
        {
            return _value!;
        }
        throw _exception!;
    }
    
    public Try<TResult> SelectMany<TResult>(Func<T, Try<TResult>> binder)
    {
        return IsSuccess ? binder(_value!) : new Try<TResult>(_exception!);
    }
}
