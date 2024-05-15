// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

// Use to encapsulate (and then potentially process with Match()) the outcome of a separate validation method! 
public record Validation<T>
{
    internal T? Value { get; }
    internal IReadOnlyList<string> Errors { get; }
    
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
    
    // Synchronous binding of synchronous operations
    public Validation<TResult> SelectMany<TResult>(Func<T, Validation<TResult>> binder)
    {
        return IsValid ? binder(Value!) : Validation<TResult>.Invalid(Errors.ToList());
    }
    
    // Combining two synchronous operations to produce a final result
    public Validation<TResult> SelectMany<TCollection, TResult>(
        Func<T, Validation<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!IsValid)
            return Validation<TResult>.Invalid(Errors.ToList());

        var collectionValidation = collectionSelector(Value!);

        return collectionValidation.IsValid
            ? Validation<TResult>.Valid(resultSelector(Value!, collectionValidation.Value!))
            : Validation<TResult>.Invalid(collectionValidation.Errors.ToList());
    }
}

public static class ValidationExtensions
{
    // Asynchronous binding of asynchronous operations:
    public static async Task<Validation<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Validation<TSource>> sourceTask,
        Func<TSource, Task<Validation<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsValid)
            return Validation<TResult>.Invalid(source.Errors.ToList());

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsValid
            ? Validation<TResult>.Valid(resultSelector(source.Value!, collection.Value!))
            : Validation<TResult>.Invalid(collection.Errors.ToList());
    }

    // Asynchronous initial operation binding to a synchronous subsequent operation
    public static async Task<Validation<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Validation<TSource>> sourceTask,
        Func<TSource, Validation<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsValid)
            return Validation<TResult>.Invalid(source.Errors.ToList());

        var collection = collectionSelector(source.Value!);

        return collection.IsValid
            ? Validation<TResult>.Valid(resultSelector(source.Value!, collection.Value!))
            : Validation<TResult>.Invalid(collection.Errors.ToList());
    }

    // Synchronous initial operation binding to an asynchronous subsequent operation
    public static async Task<Validation<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Validation<TSource> source,
        Func<TSource, Task<Validation<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (!source.IsValid)
            return Validation<TResult>.Invalid(source.Errors.ToList());

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsValid
            ? Validation<TResult>.Valid(resultSelector(source.Value!, collection.Value!))
            : Validation<TResult>.Invalid(collection.Errors.ToList());
    }
}