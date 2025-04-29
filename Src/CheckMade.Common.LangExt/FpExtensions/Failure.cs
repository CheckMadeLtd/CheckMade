namespace CheckMade.Common.LangExt.FpExtensions;

public abstract record Failure
{
    /// <summary>
    /// Only use when you
    /// a) either don't care about showing the translation of a potential BusinessError message OR
    /// b) are certain that this Failure will be an Exception and not a BusinessError 
    /// </summary>
    public abstract string GetEnglishMessage();
}

public sealed record ExceptionWrapper(Exception Exception) : Failure
{
    public override string GetEnglishMessage() => Exception.Message;
}

public sealed record BusinessError(UiString Error) : Failure
{
    public override string GetEnglishMessage() => Error.GetFormattedEnglish();
}
