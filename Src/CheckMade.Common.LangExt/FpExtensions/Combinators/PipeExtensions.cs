namespace CheckMade.Common.LangExt.FpExtensions.Combinators;

public static class PipeExtensions
{
    /// <summary>
    /// Apply a transformation to this object with a result of type TOut.
    /// </summary>
    public static TOut Pipe<TIn, TOut>(this TIn @this, Func<TIn, TOut> f) => f(@this);
    
    /// <summary>
    /// Apply a series of transformations to this object, each with the same input and output type.
    /// </summary>
    public static T Pipe<T>(this T @this, params Func<T, T>[] transformations) => 
        transformations.Aggregate(@this, static (current, func) => func(current));
}