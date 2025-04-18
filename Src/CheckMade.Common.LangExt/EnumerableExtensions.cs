using System.Collections.Immutable;

namespace CheckMade.Common.LangExt;

public static class EnumerableExtensions
{
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
        
        return result.ToImmutableList();
    }
}
