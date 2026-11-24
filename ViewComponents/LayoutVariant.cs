namespace WebApplication1.ViewComponents;

/// <summary>
/// Full = storefront pages; Compact = sign-in pages and the back office. Pages pick one with
/// <c>ViewData["LayoutVariant"] = LayoutVariant.Compact</c>; the default is Full.
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
