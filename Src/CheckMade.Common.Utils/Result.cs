namespace CheckMade.Common.Utils;

public record Result<T>
{
    public T? Value { get; }
    public bool Success { get; }
    public string? Error { get; }

    // Implicit conversion from T to Result<T>
    public static implicit operator Result<T>(T value) => new Result<T>(value);

    // Implicit conversion from string to Result<T> (for errors)
    public static implicit operator Result<T>(string error) => new Result<T>(error);

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

    // Deconstruct method for pattern matching
    public void Deconstruct(out bool success, out T? value, out string? error)
    {
        success = Success;
        value = Value;
        error = Error;
    }

    // Utility method to handle success and error cases
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onError)
    {
        return Success ? onSuccess(Value!) : onError(Error!);
    }

    public T GetValueOrThrow()
    {
        if (Success)
        {
            return Value!;
        }
        throw new InvalidOperationException(Error);
    }

    public T GetValueOrDefault(T defaultValue = default!)
    {
        return Success ? Value! : defaultValue;
    }
}

// Example usage
public class ResultUsageExamples
{
    public void Demo()
    {
        Result<int> successResult = 42;
        Result<int> errorResult = "Something went wrong";

        var value1 = successResult.GetValueOrDefault();
        var value2 = errorResult.GetValueOrDefault(-1);

        Console.WriteLine($"Value1: {value1}, Value2: {value2}");

        // Using Match method
        var message1 = successResult.Match(
            value => $"Success with value: {value}",
            error => $"Error: {error}"
        );

        var message2 = errorResult.Match(
            value => $"Success with value: {value}",
            error => $"Error: {error}"
        );

        Console.WriteLine(message1);
        Console.WriteLine(message2);
    }
}
