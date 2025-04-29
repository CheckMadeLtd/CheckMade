namespace CheckMade.Common.LangExt.FpExtensions;

public abstract record Failure
{
    public abstract string Message { get; }
}

public sealed record ExceptionWrapper(Exception Exception) : Failure
{
    public override string Message => Exception.Message;
}

public sealed record BusinessError(UiString Error) : Failure
{
    public override string Message => Error.GetFormattedEnglish();
}
