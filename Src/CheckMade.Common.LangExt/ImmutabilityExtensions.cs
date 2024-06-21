using System.Collections.Immutable;

namespace CheckMade.Common.LangExt;

public static class ImmutabilityExtensions
{
    public static IReadOnlyCollection<T> ToImmutableReadOnlyCollection<T>(this IEnumerable<T> enumerable) => 
        enumerable.ToImmutableList();
    
    public static IReadOnlyList<T> ToImmutableReadOnlyList<T>(this IEnumerable<T> enumerable) => 
        enumerable.ToImmutableList();
    
    public static IReadOnlySet<T> ToImmutableReadOnlyHashSet<T>(this IEnumerable<T> enumerable) => 
        enumerable.ToImmutableHashSet();
    
    public static IReadOnlySet<T> ToImmutableReadOnlySortedSet<T>(this IEnumerable<T> enumerable) => 
        enumerable.ToImmutableSortedSet();

    public static IReadOnlyDictionary<TKey, TValue> 
        ToImmutableReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull =>
        dictionary.ToImmutableDictionary();
    
    public static IReadOnlyDictionary<TKey, TValue> 
        ToImmutableReadOnlySortedDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull =>
        dictionary.ToImmutableSortedDictionary();
}
