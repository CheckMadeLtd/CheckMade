using CheckMade.Common.LangExt.FpExtensions.MonadicWrappers;

namespace CheckMade.Common.LangExt.FpExtensions.Combinators;

public static class AlternateExtensions
{
    /// <summary>
    ///  Try a set of functions (to achieve the same end) that could result in null until the first one succeeds.
    /// </summary>
    public static TOut Alternate<TIn, TOut>(
        this TIn @this,
        params Func<TIn, TOut>[] altFuncs) =>
        altFuncs.Select(x => x(@this))
            .First(static x => x is not null);

    /// <summary>
    ///  Try a set of functions (to achieve the same end) that could result in no value until the first one succeeds.
    /// </summary>
    public static Option<TOut> Alternate<TIn, TOut>(
        this TIn @this,
        params Func<TIn, Option<TOut>>[] altFuncs) =>
        altFuncs.Select(x => x(@this))
            .First(static x => x.IsSome);
    
    /// <summary>
    ///  Try a set of functions (to achieve the same end) that could result in an error until the first one succeeds.
    /// </summary>
    public static Result<TOut> Alternate<TIn, TOut>(
        this TIn @this,
        params Func<TIn, Result<TOut>>[] altFuncs) =>
        altFuncs.Select(x => x(@this))
            .First(static x => x.IsSuccess);
    
    /// <summary>
    ///  Try a set of functions (to achieve the same end) that could throw an exception until the first one succeeds.
    /// </summary>
    public static Attempt<TOut> Alternate<TIn, TOut>(
        this TIn @this,
        params Func<TIn, Attempt<TOut>>[] altFuncs) =>
        altFuncs.Select(x => x(@this))
            .First(static x => x.IsSuccess);
}