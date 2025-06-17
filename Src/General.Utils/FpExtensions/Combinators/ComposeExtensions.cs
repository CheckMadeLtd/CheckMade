namespace General.Utils.FpExtensions.Combinators;

public static class ComposeExtensions
{
    /// <summary>
    /// The output of this func is piped into the new func provided as the argument.
    /// </summary>
    public static Func<TIn, TNewOut> Compose<TIn, TOldOut, TNewOut>(
        this Func<TIn, TOldOut> @this,
        Func<TOldOut, TNewOut> f) => x 
        => f(@this(x));

    /// <summary>
    /// Applies (LINQ) filters/transformations to this IEnumerable and then uses the aggregator to return the final value. 
    /// </summary>
    public static TFinalOut Transduce<TIn, TFilterOut, TFinalOut>(
        this IEnumerable<TIn> @this,
        Func<IEnumerable<TIn>, IEnumerable<TFilterOut>> transformer,
        Func<IEnumerable<TFilterOut>, TFinalOut> aggregator) =>
        aggregator(transformer(@this));

    /// <summary>
    ///  Composing this transformer/filter func into a transducer by adding the aggregator to the pipe.
    /// </summary>
    public static Func<IEnumerable<TIn>, TAggregated> ToTransducer<TIn, TTransformed, TAggregated>(
        this Func<IEnumerable<TIn>, IEnumerable<TTransformed>> @this,
        Func<IEnumerable<TTransformed>, TAggregated> aggregator) =>
        x => aggregator(@this(x));

    /// <summary>
    /// Monitor or report on this e.g. in the middle of a functions pipeline without interrupting it. 
    /// </summary>
    public static T Tap<T>(this T @this, Action<T> sideEffect)
    {
        sideEffect(@this);
        return @this;
    }
}