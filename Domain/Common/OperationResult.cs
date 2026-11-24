namespace WebApplication1.Domain.Common;

/// <summary>
/// เก็บผลสำเร็จพร้อมข้อมูล หรือผลล้มเหลวพร้อมข้อความที่ผู้ใช้อ่านได้
/// บริการคืนผลชนิดนี้สำหรับกรณีที่คาดไว้ เช่น สินค้าหมดหรือคูปองใช้ไม่ได้
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
