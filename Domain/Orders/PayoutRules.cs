namespace WebApplication1.Domain.Orders;

public static class PayoutRules
{
    public const int MaxReferenceLength = 100;

    public static bool CanApprove(OrderStatus status, bool commissionDeducted, bool hasPayout) =>
        status == OrderStatus.Delivered && commissionDeducted && !hasPayout;

    public static bool IsValidTransfer(DateOnly? date, DateOnly today, string? reference) =>
        date is { } day && day > DateOnly.MinValue && day <= today &&
        !string.IsNullOrWhiteSpace(reference) && reference.Trim().Length <= MaxReferenceLength;

    public static decimal Commission(decimal total, decimal rate) => Math.Round(total * rate, 2, MidpointRounding.AwayFromZero);

    public static string StatusLabel(bool commissionDeducted, bool approved, bool transferred) =>
        transferred ? "โอนแล้ว" : approved ? "อนุมัติแล้ว รอโอน" : commissionDeducted ? "รออนุมัติ" : "รอหักคอม";
}
