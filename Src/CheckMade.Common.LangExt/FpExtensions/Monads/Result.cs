
namespace CheckMade.Common.LangExt.FpExtensions.Monads;

public sealed record Result<T>
{
    internal T? Value { get; }
    internal Failure? FailureInfo { get; }

    public bool IsSuccess => FailureInfo == null;
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        Value = value;
        FailureInfo = null;
    }

    private Result(Failure failure)
    {
        Value = default;
        FailureInfo = failure ?? throw new ArgumentNullException(nameof(failure));
    }

    public static Result<T> Succeed(T value) => new(value);
    
    public static Result<T> Fail(Exception exception) => new(new ExceptionWrapper(exception));
    public static Result<T> Fail(UiString error) => new(new BusinessError(error));

    public static Result<T> Fail(Failure failure) =>
        failure switch
        {
            ExceptionWrapper ex => Fail(ex.Exception),
            BusinessError error => Fail(error.Error),
            _ => throw new InvalidOperationException("Unknown failure type")
        };

    public static implicit operator Result<T>(T value) => Succeed(value);  
    public static implicit operator Result<T>(Exception ex) => Fail(ex);
    public static implicit operator Result<T>(UiString error) => Fail(error);
    public static implicit operator Result<T>(Failure failure) => Fail(failure);

    public static Result<T> Run(Func<T> func)
    {
        try
        {
            return Succeed(func());
        }
        catch (Exception ex)
        {
            return Fail(ex);
        }
    }
    
    public static async Task<Result<T>> RunAsync(Func<Task<T>> func)
    {
        try
        {
            return Succeed(await func());
        }
        // also covers any AggregateException (e.g. from `await Task.WhenAll()`
        catch (Exception ex)
        {
            return Fail(ex);
        }
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Failure, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(FailureInfo!);

    public T GetValueOrDefault(T defaultValue = default!) =>
        IsSuccess ? Value! : defaultValue;

    public T GetValueOrThrow() =>
        IsSuccess switch
        {
            true => Value!,
            _ => FailureInfo switch
            {
                ExceptionWrapper exFailure => throw exFailure.Exception,
                BusinessError error => throw new InvalidOperationException(error.Error.GetFormattedEnglish()),
                _ => throw new InvalidOperationException("Unknown failure type")
            }
        };
}