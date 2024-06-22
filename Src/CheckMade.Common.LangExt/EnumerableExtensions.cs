using System.Collections.Immutable;

namespace CheckMade.Common.LangExt;

public static class EnumerableExtensions
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
    
    public static IReadOnlyCollection<T> GetLatestRecordsUpTo<T>(
        this IEnumerable<T> enumerable, Func<T, bool> stopCondition, bool includeStopItem = true)
    {
        var enumeratedDesc = enumerable.Reverse().ToList(); // .Reverse() required for usage of .TakeWhile()
        
        if (enumeratedDesc.Count == 0)
            return ImmutableList<T>.Empty;
        
        var result = enumeratedDesc
            .TakeWhile(item => !stopCondition(item))
            .ToList();

        if (includeStopItem)
        {
            var firstItemMeetingCondition = enumeratedDesc.FirstOrDefault(stopCondition);

            if (firstItemMeetingCondition != null)
                result.Add(firstItemMeetingCondition);
        }

        result.Reverse(); // back to the original ASC order
        
        return result.ToImmutableReadOnlyCollection();
    }
}
