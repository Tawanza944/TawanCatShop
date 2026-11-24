using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebApplication1.TagHelpers;

/// <summary>
/// ทำเครื่องหมายลิงก์เมนูที่ตรงกับหน้าปัจจุบัน
/// ตัวอย่าง <c>&lt;a asp-controller="Home" asp-action="Product" lc-active-when="Home/Product,ProductDetail"&gt;</c>
/// ค่าที่ระบุเป็นคู่ Controller/Action หากมีเฉพาะ Action ให้ใช้ Controller จากรายการก่อน
/// </summary>
[HtmlTargetElement("a", Attributes = AttributeName)]
public sealed class ActiveRouteTagHelper : TagHelper
{
    private const string AttributeName = "lc-active-when";

    [HtmlAttributeName(AttributeName)]
    public string ActiveWhen { get; set; } = string.Empty;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();

        if (Matches(currentController, currentAction))
        {
            output.Attributes.SetAttribute("aria-current", "page");
        }
    }

    private bool Matches(string? controller, string? action)
    {
        string? lastController = null;
        foreach (var part in ActiveWhen.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pieces = part.Split('/');
            var (expectedController, expectedAction) = pieces.Length == 2 ? (pieces[0], pieces[1]) : (lastController, pieces[0]);
            lastController = expectedController;

            if (string.Equals(expectedController, controller, StringComparison.OrdinalIgnoreCase)
                && (expectedAction == "*" || string.Equals(expectedAction, action, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
