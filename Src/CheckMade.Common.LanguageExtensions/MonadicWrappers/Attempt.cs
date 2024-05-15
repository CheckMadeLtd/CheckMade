// ReSharper disable MemberCanBePrivate.Global
namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

public record Attempt<T>
{
    private readonly T? _value;
    private readonly Exception? _exception;
    
    public bool IsSuccess => _exception == null;
    public bool IsFailure => !IsSuccess;

    private Attempt(T value)
    {
        _value = value;
        _exception = null;
    }

    private Attempt(Exception exception)
    {
        _exception = exception;
        _value = default;
    }

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
        return IsSuccess ? onSuccess(_value!) : onFailure(_exception!);
    }

    public static Attempt<T> Fail(Exception exception)
    {
        return new Attempt<T>(exception);
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
    
    public Attempt<TResult> SelectMany<TResult>(Func<T, Attempt<TResult>> binder)
    {
        return IsSuccess ? binder(_value!) : new Attempt<TResult>(_exception!);
    }
}
