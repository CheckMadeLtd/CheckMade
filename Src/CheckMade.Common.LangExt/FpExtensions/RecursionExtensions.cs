namespace CheckMade.Common.LangExt.FpExtensions;

public static class RecursionExtensions
{
    /// <summary>
    /// Trampolining via thunk + while(true) + endCondition 
    /// </summary>
    public static T IterateUntil<T>(
        this T @this,
        Func<T, T> updateFunc,
        Func<T, bool> endCondition)
    {
        var currentThis = @this;
        while (!endCondition(currentThis))
        {
            currentThis = updateFunc(currentThis);
        }

        return currentThis;
    }
}