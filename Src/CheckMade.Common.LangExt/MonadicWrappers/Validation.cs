// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.LangExt.MonadicWrappers;

// Use to encapsulate (and then potentially process with Match()) the outcome of a separate validation method! 
public record Validation<T>
{
    internal T? Value { get; }
    internal IReadOnlyList<UiString> Errors { get; }

    public bool IsValid => Errors.Count == 0;
    public bool IsInvalid => !IsValid;

    private Validation(T value)
    {
        Value = value;
        Errors = new List<UiString>();
    }

    private Validation(List<UiString> errors)
    {
        Value = default;
        Errors = errors;
    }

    public static Validation<T> Valid(T value) => new(value);
    public static Validation<T> Invalid(List<UiString> errors) => new(errors);
    public static Validation<T> Invalid(params UiString[] errors) => new(errors.ToList());

    public TResult Match<TResult>(Func<T, TResult> onValid, Func<IReadOnlyList<UiString>, TResult> onInvalid)
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
}

public static class ValidationExtensions
{
    public static Validation<TResult> Select<T, TResult>(this Validation<T> source, Func<T, TResult> selector)
    {
        return source.IsValid 
            ? Validation<TResult>.Valid(selector(source.Value!)) 
            : Validation<TResult>.Invalid(source.Errors.ToList());
    }

    public static Validation<T> Where<T>(this Validation<T> source, Func<T, bool> predicate)
    {
        if (!source.IsValid) return source;

        return predicate(source.Value!) 
            ? source 
            : Validation<T>.Invalid(UiNoTranslate("Predicate not satisfied"));
    }

    // Synchronous binding of synchronous operations
    public static Validation<TResult> SelectMany<T, TResult>(
        this Validation<T> source,
        Func<T, Validation<TResult>> binder)
    {
        return source.IsValid ? binder(source.Value!) : Validation<TResult>.Invalid(source.Errors.ToList());
    }
    
    // Combining two synchronous operations to produce a final result
    public static Validation<TResult> SelectMany<T, TCollection, TResult>(
        this Validation<T> source,
        Func<T, Validation<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!source.IsValid)
            return Validation<TResult>.Invalid(source.Errors.ToList());

        var collectionValidation = collectionSelector(source.Value!);

        return collectionValidation.IsValid
            ? Validation<TResult>.Valid(resultSelector(source.Value!, collectionValidation.Value!))
            : Validation<TResult>.Invalid(collectionValidation.Errors.ToList());
    }
    
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