using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApplication1.Domain.Catalog;
using WebApplication1.Domain.Orders;
using WebApplication1.Options;

namespace WebApplication1.ViewComponents;

public sealed record FooterViewModel(
    StoreInfoOptions Store,
    IReadOnlyList<StorefrontCategory> Categories,
    IReadOnlyList<PaymentMethod> PaymentMethods,
    decimal FreeShippingThreshold);

/// <summary>เตรียมข้อมูลท้ายเว็บจาก StoreInfo ใน appsettings ผ่าน Options</summary>
public sealed class FooterViewComponent : ViewComponent
{
    private readonly StoreInfoOptions _store;
    private readonly CommerceOptions _commerce;

    public FooterViewComponent(IOptions<StoreInfoOptions> store, IOptions<CommerceOptions> commerce)
    {
        _store = store.Value;
        _commerce = commerce.Value;
    }

    public IViewComponentResult Invoke(bool compact = false)
    {
        var model = new FooterViewModel(_store, StorefrontCategories.All, PaymentMethods.All, _commerce.FreeShippingThreshold);
        return View(compact ? "Compact" : "Default", model);
    }
}
