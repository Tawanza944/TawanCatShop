namespace WebApplication1.Domain.Common;

/// <summary>
/// Outcome of a business operation: either success with a value, or a message the user can read.
/// Services return this instead of throwing for expected failures (out of stock, invalid coupon...).
/// </summary>
public sealed class OperationResult<T>
{
    private OperationResult(bool succeeded, T? value, string? error)
    {
        Succeeded = succeeded;
        Value = value;
        Error = error;
    }

    public bool Succeeded { get; }

    public T? Value { get; }

    public string? Error { get; }

    public static OperationResult<T> Success(T value) => new(true, value, null);

    public static OperationResult<T> Failure(string error) => new(false, default, error);
}

public static class OperationResult
{
    public static OperationResult<bool> Ok() => OperationResult<bool>.Success(true);

    public static OperationResult<bool> Fail(string error) => OperationResult<bool>.Failure(error);
}
