// ReSharper disable MemberCanBePrivate.Global
namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

// Use to encapsulate (and then potentially process with Match()) the outcome of a separate validation method! 
public record Validation<T>
{
    public T? Value { get; }
    public IReadOnlyList<string> Errors { get; }
    
    public bool IsValid { get; }
    public bool IsInvalid => !IsValid;

    private Validation(T value)
    {
        Value = value;
        Errors = new List<string>();
        IsValid = true;
    }

    private Validation(List<string> errors)
    {
        Value = default;
        Errors = errors;
        IsValid = false;
    }

    public static Validation<T> Valid(T value) => new Validation<T>(value);

    public static Validation<T> Invalid(List<string> errors) => new Validation<T>(errors);

    public static Validation<T> Invalid(params string[] errors) => new Validation<T>(errors.ToList());

    public TResult Match<TResult>(Func<T, TResult> onValid, Func<IReadOnlyList<string>, TResult> onInvalid)
    {
        return IsValid ? onValid(Value!) : onInvalid(Errors);
    }

    public T GetValueOrThrow()
    {
        if (IsValid)
        {
            return Value!;
        }
        throw new InvalidOperationException(string.Join(", ", Errors));
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsValid ? Value! : defaultValue;
    }
    
    public Validation<TResult> SelectMany<TResult>(Func<T, Validation<TResult>> binder)
    {
        return IsValid ? binder(Value!) : Validation<TResult>.Invalid(Errors.ToList());
    }
}
