namespace WebApplication1.ViewComponents;

/// <summary>
/// Full ใช้กับหน้าร้าน ส่วน Compact ใช้กับหน้าเข้าสู่ระบบและหลังบ้าน
/// เลือกด้วย <c>ViewData["LayoutVariant"] = LayoutVariant.Compact</c> ค่าเริ่มต้นคือ Full
/// </summary>
public enum LayoutVariant
{
    Full,
    Compact
}

public static class LayoutVariantResolver
{
    public const string ViewDataKey = "LayoutVariant";

    public static LayoutVariant From(IDictionary<string, object?> viewData) =>
        viewData.TryGetValue(ViewDataKey, out var value) && value is LayoutVariant variant ? variant : LayoutVariant.Full;
}
