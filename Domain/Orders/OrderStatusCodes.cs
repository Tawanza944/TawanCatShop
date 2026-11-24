namespace WebApplication1.Domain.Orders;

/// <summary>
/// รวมการแปลง <see cref="OrderStatus"/> เป็นรหัสฐานข้อมูลและชื่อภาษาไทยไว้ที่เดียว
/// กำหนดรหัสไว้ชัดเจนเพื่อให้เปลี่ยนชื่อสมาชิก enum ได้โดยคงรหัสที่จัดเก็บ
/// </summary>
public static class OrderStatusCodes
{
    private static readonly IReadOnlyDictionary<OrderStatus, string> Codes = new Dictionary<OrderStatus, string>
    {
        [OrderStatus.Pending] = "pending",
        [OrderStatus.Packing] = "packing",
        [OrderStatus.Ready] = "ready",
        [OrderStatus.Delivering] = "delivering",
        [OrderStatus.Delivered] = "delivered",
        [OrderStatus.Cancelled] = "cancelled"
    };

    private static readonly IReadOnlyDictionary<string, OrderStatus> ByCode =
        Codes.ToDictionary(pair => pair.Value, pair => pair.Key, StringComparer.OrdinalIgnoreCase);

    public static string ToCode(this OrderStatus status) => Codes[status];

    public static bool TryParseCode(string? code, out OrderStatus status) =>
        ByCode.TryGetValue(code?.Trim() ?? string.Empty, out status);

    public static OrderStatus ParseCode(string code) =>
        TryParseCode(code, out var status)
            ? status
            : throw new FormatException($"Unknown order status code '{code}'.");

    public static string ToThai(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => "รอร้านยืนยัน",
        OrderStatus.Packing => "กำลังแพ็กของ",
        OrderStatus.Ready => "พร้อมส่ง",
        OrderStatus.Delivering => "กำลังจัดส่ง",
        OrderStatus.Delivered => "ได้รับของแล้ว",
        OrderStatus.Cancelled => "ยกเลิกแล้ว",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
    };

    /// <summary>ลำดับในแถบติดตามของลูกค้าเริ่มที่ 1 ส่วนออเดอร์ยกเลิกใช้ค่าก่อนขั้นแรก</summary>
    public static int ToStepIndex(this OrderStatus status) => status switch
    {
        OrderStatus.Pending => 1,
        OrderStatus.Packing => 2,
        OrderStatus.Ready => 2,
        OrderStatus.Delivering => 3,
        OrderStatus.Delivered => 4,
        _ => 0
    };
}
