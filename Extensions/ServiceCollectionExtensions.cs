using WebApplication1.Domain.Orders;
using WebApplication1.Options;
using WebApplication1.Services.Accounts;
using WebApplication1.Services.Backoffice;
using WebApplication1.Services.Catalog;
using WebApplication1.Services.Dashboards;
using WebApplication1.Services.Identity;
using WebApplication1.Services.Orders;
using WebApplication1.Services.Storage;

namespace WebApplication1.Extensions;

/// <summary>
/// ลงทะเบียนบริการของ LoveCat ไว้ที่นี่ เพื่อให้ Program.cs แสดงลำดับเริ่มต้นแอปได้ชัดเจน
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLoveCatApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton(TimeProvider.System);

        services.Configure<CommerceOptions>(configuration.GetSection(CommerceOptions.SectionName));
        services.Configure<StoreInfoOptions>(configuration.GetSection(StoreInfoOptions.SectionName));

        services.AddScoped<ICurrentUserService, SessionCurrentUserService>();
        services.AddScoped<IAuthSession, HttpSessionAuth>();

        services.AddSingleton<IOrderWorkflow, OrderWorkflow>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IOrderNumberGenerator, DailyOrderNumberGenerator>();
        services.AddScoped<ICheckoutService, CheckoutService>();
        services.AddScoped<BasketCancellationService>();
        services.AddScoped<IOrderFulfillmentService, OrderFulfillmentService>();
        services.AddScoped<IShipmentService, ShipmentService>();

        services.AddScoped<IProductCatalogService, ProductCatalogService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<CouponCollection>();

        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IImageStorage, LocalImageStorage>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ShopProfileService>();
        services.AddScoped<ShopInteractions>();
        services.AddScoped<FlashSaleAdmin>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<ICouponAdminService, CouponAdminService>();
        services.AddScoped<IAccountingService, AccountingService>();

        // ใช้ Strategy แยก Dashboard ตาม Role แล้วให้ AccountController เลือกชุดที่ตรงกับผู้ใช้
        services.AddScoped<IRoleDashboard, AdminDashboard>();
        services.AddScoped<IRoleDashboard, ShopDashboard>();
        services.AddScoped<IRoleDashboard, RaiderDashboard>();
        services.AddScoped<IRoleDashboard, AccountingDashboard>();
        services.AddScoped<IRoleDashboard, OwnerDashboard>();
        services.AddScoped<IRoleDashboard, CustomerDashboard>();

        return services;
    }
}
