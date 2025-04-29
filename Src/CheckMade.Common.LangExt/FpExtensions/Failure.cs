namespace CheckMade.Common.LangExt.FpExtensions;

public abstract record Failure;

public sealed record ExceptionWrapper(Exception Exception) : Failure;

public sealed record BusinessError(UiString Error) : Failure;
