namespace WebApplication1.Domain.Pricing;

/// <summary>ค่าประเภทส่วนลดที่บันทึกใน <c>coupons.discount_type</c></summary>
public static class CouponTypes
{
    public const string Fixed = "Fixed";
    public const string Percent = "Percent";
    public const string FreeShipping = "FreeShipping";

    public static IReadOnlyList<string> All { get; } = [Fixed, Percent, FreeShipping];

    public static string ToThai(string type) => type switch
    {
        Fixed => "ลดเป็นบาท",
        Percent => "ลดเป็นเปอร์เซ็นต์",
        FreeShipping => "ส่งฟรี",
        _ => type
    };
}
