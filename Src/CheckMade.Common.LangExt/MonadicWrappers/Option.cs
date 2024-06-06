// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeInternal
namespace CheckMade.Common.LangExt.MonadicWrappers;

public record Option<T>
{
    public T? Value { get; }
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
        throw new InvalidOperationException("No value present");
    }
}

public static class OptionExtensions
{
    public static Option<TResult> Select<T, TResult>(this Option<T> source, Func<T, TResult> selector)
    {
        return source.IsSome 
            ? Option<TResult>.Some(selector(source.Value!)) 
            : Option<TResult>.None();
    }

    public static Option<T> Where<T>(this Option<T> source, Func<T, bool> predicate)
    {
        if (!source.IsSome) return Option<T>.None();

        return predicate(source.Value!) 
            ? source 
            : Option<T>.None();
    }
    
    // Synchronous binding of synchronous operations
    public static Option<TResult> SelectMany<T, TResult>(this Option<T> source, Func<T, Option<TResult>> binder)
    {
        return source.IsSome ? binder(source.Value!) : Option<TResult>.None();
    }
    
    // Combining two synchronous operations to produce a final result
    public static Option<TResult> SelectMany<T, TCollection, TResult>(
        this Option<T> source,
        Func<T, Option<TCollection>> collectionSelector,
        Func<T, TCollection, TResult> resultSelector)
    {
        if (!source.IsSome)
            return Option<TResult>.None();

        var collectionOption = collectionSelector(source.Value!);

        return collectionOption.IsSome
            ? Option<TResult>.Some(resultSelector(source.Value!, collectionOption.Value!))
            : Option<TResult>.None();
    }
    
    // // Asynchronous binding of asynchronous operations
    public static async Task<Option<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Option<TSource>> sourceTask,
        Func<TSource, Task<Option<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;
    
        if (!source.IsSome)
            return Option<TResult>.None();
    
        var collection = await collectionTaskSelector(source.Value!);
    
        return collection.IsSome
            ? Option<TResult>.Some(resultSelector(source.Value!, collection.Value!))
            : Option<TResult>.None();
    }
    
    // // Asynchronous initial operation binding to a synchronous subsequent operation
    public static async Task<Option<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Option<TSource>> sourceTask,
        Func<TSource, Option<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;
    
        if (!source.IsSome)
            return Option<TResult>.None();
    
        var collection = collectionSelector(source.Value!);
    
        return collection.IsSome
            ? Option<TResult>.Some(resultSelector(source.Value!, collection.Value!))
            : Option<TResult>.None();
    }
    
    // Synchronous initial operation binding to an asynchronous subsequent operation
    public static async Task<Option<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Option<TSource> source,
        Func<TSource, Task<Option<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (!source.IsSome)
            return Option<TResult>.None();
    
        var collection = await collectionTaskSelector(source.Value!);
    
        return collection.IsSome
            ? Option<TResult>.Some(resultSelector(source.Value!, collection.Value!))
            : Option<TResult>.None();
    }
    
    // Asynchronous initial operation binding to a synchronous subsequent operation
}