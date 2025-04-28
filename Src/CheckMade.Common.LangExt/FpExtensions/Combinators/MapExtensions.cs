namespace CheckMade.Common.LangExt.FpExtensions.Combinators;

public static class MapExtensions
{
    /// <summary>
    /// Apply a transformation to this object with a result of type TOut.
    /// </summary>
    public static TOut Map<TIn, TOut>(this TIn @this, Func<TIn, TOut> f) => f(@this);
    
    /// <summary>
    /// Apply a series of transformations to this object, each with the same input and output type.
    /// </summary>
    public static T Map<T>(this T @this, params Func<T, T>[] transformations) => 
        transformations.Aggregate(@this, static (current, func) => func(current));
}