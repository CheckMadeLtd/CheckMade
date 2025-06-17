using System.Collections.Concurrent;

namespace General.Utils.FpExtensions;

public static class MemoizationExtensions
{
    /// <summary>
    /// Cache calculations temporarily in a thread-safe way to avoid repeating them in parallelized, intensive
    /// computations. Use this overload for input types suitable as Dict keys. 
    /// </summary>
    public static Func<T, TResult> Memoize<T, TResult>(this Func<T, TResult> @this)
        where T : IEquatable<T>
    {
        var cache = new ConcurrentDictionary<T, TResult>();

        // using valueFactory overload to make sure the value gets calculated only if it's not present yet in the cache!
        return x => cache.GetOrAdd(x, _ => @this(x)); 
    }
    
    /// <summary>
    /// Use this overload when input types are not suitable as Dict keys (e.g. custom, non-record classes)
    /// </summary>
    public static Func<T, TResult> Memoize<T, TKey, TResult>(
        this Func<T, TResult> @this,
        Func<T, TKey> keySelector)
        where TKey : IEquatable<TKey>
    {
        var cache = new ConcurrentDictionary<TKey, TResult>();

        return x => cache.GetOrAdd(keySelector(x), _ => @this(x));
    }
    
    // Add versions of Memoize with more than one input parameter if needed
}