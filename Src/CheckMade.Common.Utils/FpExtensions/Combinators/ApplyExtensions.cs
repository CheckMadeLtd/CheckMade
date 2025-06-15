namespace CheckMade.Common.Utils.FpExtensions.Combinators;

public static class ApplyExtensions
{
    /// <summary>
    /// Apply a transformation to this object with a result of type TOut.
    /// </summary>
    public static TOut Apply<TIn, TOut>(this TIn @this, Func<TIn, TOut> f) => f(@this);
    
    /// <summary>
    /// Apply a series of transformations to this object, each with the same input and output type.
    /// </summary>
    public static T Apply<T>(this T @this, params Func<T, T>[] transformations) => 
        transformations.Aggregate(@this, static (current, func) => func(current));
}