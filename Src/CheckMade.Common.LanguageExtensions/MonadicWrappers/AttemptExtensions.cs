namespace CheckMade.Common.LanguageExtensions.MonadicWrappers;

public static class AttemptExtensions 
{
    /* Covers scenarios where both the initial attempt and the function it binds to are
     asynchronous operations, allowing for the combination of their results asynchronously */
    public static async Task<Attempt<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Attempt<TSource>> sourceTask,
        Func<TSource, Task<Attempt<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsSuccess)
            return Attempt<TResult>.FromException(source.Exception!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess 
            ? Attempt<TResult>.FromValue(resultSelector(source.Value!, collection.Value!))
            : Attempt<TResult>.FromException(collection.Exception!);
    }
    
    /* Covers scenarios where the initial attempt is an asynchronous operation, but the function
     it binds to is synchronous, enabling the combination of asynchronous and synchronous operations */ 
    public static async Task<Attempt<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Task<Attempt<TSource>> sourceTask,
        Func<TSource, Attempt<TCollection>> collectionSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        var source = await sourceTask;

        if (!source.IsSuccess)
            return Attempt<TResult>.FromException(source.Exception!);

        var collection = collectionSelector(source.Value!);

        return collection.IsSuccess 
            ? Attempt<TResult>.FromValue(resultSelector(source.Value!, collection.Value!))
            : Attempt<TResult>.FromException(collection.Exception!);
    }
    
    /* Handles cases where you start with a synchronous Attempt<T>, but need to perform an asynchronous operation based
     on the result */
    public static async Task<Attempt<TResult>> SelectMany<TSource, TCollection, TResult>(
        this Attempt<TSource> source,
        Func<TSource, Task<Attempt<TCollection>>> collectionTaskSelector,
        Func<TSource, TCollection, TResult> resultSelector)
    {
        if (!source.IsSuccess)
            return Attempt<TResult>.FromException(source.Exception!);

        var collection = await collectionTaskSelector(source.Value!);

        return collection.IsSuccess
            ? Attempt<TResult>.FromValue(resultSelector(source.Value!, collection.Value!))
            : Attempt<TResult>.FromException(collection.Exception!);
    }
}