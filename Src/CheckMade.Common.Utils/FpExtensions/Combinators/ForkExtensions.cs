namespace CheckMade.Common.Utils.FpExtensions.Combinators;

public static class ForkExtensions
{
    /// <summary>
    /// Process this single value in two independent ways into intermediate values,
    /// then join those up into a single, final value.
    /// </summary>
    public static TOut Fork<TIn, T1, T2, TOut>(
        this TIn @this,
        Func<TIn, T1> f1,
        Func<TIn, T2> f2,
        Func<T1, T2, TOut> joinFunc)
    {
        var prong1 = f1(@this);
        var prong2 = f2(@this);
        return joinFunc(prong1, prong2);
    }
    
    /// <summary>
    /// Process this single value in three independent ways into intermediate values,
    /// then join those up into a single, final value.
    /// </summary>
    public static TOut Fork<TIn, T1, T2, T3, TOut>(
        this TIn @this,
        Func<TIn, T1> f1,
        Func<TIn, T2> f2,
        Func<TIn, T3> f3,
        Func<T1, T2, T3, TOut> joinFunc)
    {
        var prong1 = f1(@this);
        var prong2 = f2(@this);
        var prong3 = f3(@this);
        return joinFunc(prong1, prong2, prong3);
    }

    /// <summary>
    /// Process this single value in any number of independent ways into intermediate values of the same type (TMiddle),
    /// then join those up into a single, final value.
    /// </summary>
    public static TEnd Fork<TStart, TMiddle, TEnd>(
        this TStart @this,
        Func<IEnumerable<TMiddle>, TEnd> joinFunc,
        params Func<TStart, TMiddle>[] prongs)
    {
        var intermediateValues = prongs.Select(x => x(@this));
        return joinFunc(intermediateValues);
    }
}