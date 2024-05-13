namespace CheckMade.Common.Utils;

public record Result<T>
{
    public T? Value { get; }
    public bool Success { get; }
    public string? Error { get; }

    public Result(T value)
    {
        Value = value;
        Success = true;
    }

    public Result(string error)
    {
        Error = error;
        Success = false;
    }
}