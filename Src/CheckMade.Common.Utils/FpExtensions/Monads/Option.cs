// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.Utils.FpExtensions.Monads;

public sealed record Option<T>
{
    internal T? Value { get; }
    internal bool HasValue { get; }

    public bool IsSome => HasValue;
    public bool IsNone => !HasValue;
    
    private Option(T value)
    {
        Value = value;
        HasValue = true;
    }

    private Option()
    {
        Value = default;
        HasValue = false;
    }

    // Implicit conversion from T to Option<T>
    public static implicit operator Option<T>(T value) => Some(value);
    
    public static Option<T> Some(T value) => new(value);
    public static Option<T> None() => new();

    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone)
    {
        return IsSome ? onSome(Value!) : onNone();
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return IsSome ? Value! : defaultValue;
    }

    public T GetValueOrThrow()
    {
        if (IsSome)
        {
            return Value!;
        }

        throw new InvalidOperationException(
            $"No value present for type '{typeof(T)}' even though we expected there would have to be one!");
    }
}