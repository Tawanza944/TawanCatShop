using WebApplication1.ViewModels.Store;

namespace WebApplication1.Services.Catalog;

/// <summary>บริการอ่านการ์ดสินค้า ยอดขาย ป้ายสินค้า และข้อมูลสำหรับตัวกรองหน้าร้าน</summary>
public interface IProductCatalogService
{
    Task<StorefrontShop?> GetShopAsync(int shopId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryCount>> GetShopCategoryCountsAsync(int shopId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProductCard>> GetCardsAsync(ProductQuery query, CancellationToken cancellationToken = default);

    Task<ProductCard?> GetCardAsync(int productId, CancellationToken cancellationToken = default);

    Task<string?> GetDescriptionAsync(int productId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryCount>> GetCategoryCountsAsync(CancellationToken cancellationToken = default);

    Task<StoreStats> GetStatsAsync(CancellationToken cancellationToken = default);
}
