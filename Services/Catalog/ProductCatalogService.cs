using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Domain.Catalog;
using WebApplication1.Domain.Orders;
using WebApplication1.Models;
using WebApplication1.Services.Orders;
using WebApplication1.ViewModels.Store;

namespace WebApplication1.Services.Catalog;

public sealed class ProductCatalogService : IProductCatalogService
{
    private static readonly TimeSpan NewProductWindow = TimeSpan.FromDays(14);

    private readonly AppDbContext _db;
    private readonly TimeProvider _clock;

    public ProductCatalogService(AppDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ProductCard>> GetCardsAsync(ProductQuery query, CancellationToken cancellationToken = default)
    {
        var products = _db.Products.AsNoTracking().Include(p => p.Shop).ThenInclude(p => p!.Shop).Where(p => p.Shop != null);

        if (query.ShopId is int shopId)
        {
            products = products.Where(p => p.Shop!.ShopId == shopId);
        }

        if (StorefrontCategories.FindByKey(query.CategoryKey) is { } category)
        {
            products = products.Where(p => p.Category == category.Name);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            products = products.Where(p => p.Name.Contains(term) || (p.Description != null && p.Description.Contains(term)));
        }

        var cards = (await ToCardsAsync(await products.ToListAsync(cancellationToken), cancellationToken))
            .Where(c => (query.MinPrice is null || c.Price >= query.MinPrice) &&
                (query.MaxPrice is null || c.Price <= query.MaxPrice));

        return query.Sort switch
        {
            ProductSort.PriceLow => cards.OrderBy(c => c.Price).ToList(),
            ProductSort.PriceHigh => cards.OrderByDescending(c => c.Price).ToList(),
            ProductSort.BestSelling => cards.OrderByDescending(c => c.Sold).ThenByDescending(c => c.Rating).ToList(),
            ProductSort.Newest => cards.OrderByDescending(c => c.Id).ToList(),
            _ => cards.OrderByDescending(c => c.InStock).ThenByDescending(c => c.Badge != ProductBadge.None).ThenByDescending(c => c.Sold).ThenByDescending(c => c.Id).ToList()
        };
    }

    public async Task<ProductCard?> GetCardAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.AsNoTracking().Include(p => p.Shop).ThenInclude(p => p!.Shop).FirstOrDefaultAsync(p => p.Id == productId && p.Shop != null, cancellationToken);
        return product is null ? null : (await ToCardsAsync([product], cancellationToken)).Single();
    }

    public Task<string?> GetDescriptionAsync(int productId, CancellationToken cancellationToken = default) =>
        _db.Products.AsNoTracking().Where(p => p.Id == productId).Select(p => p.Description).FirstOrDefaultAsync(cancellationToken);

    public Task<StorefrontShop?> GetShopAsync(int shopId, CancellationToken cancellationToken = default) =>
        _db.Set<MarketplaceShop>().AsNoTracking().Where(s => s.Id == shopId)
            .Select(s => new StorefrontShop(s.Id, s.Name, _db.Set<ShopProfileImage>()
                .Where(image => image.ShopId == s.Id).Select(image => image.ImageUrl).FirstOrDefault(),
                _db.Set<ShopFollow>().Count(f => f.ShopId == s.Id),
                _db.ProductReviews.Where(r => _db.Set<ProductShop>().Any(p => p.ProductId == r.ProductId && p.ShopId == s.Id))
                    .Average(r => (double?)r.Rating)))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<IReadOnlyList<CategoryCount>> GetShopCategoryCountsAsync(int shopId, CancellationToken cancellationToken = default) =>
        CountCategoriesAsync(_db.Products.Where(p => p.Shop != null && p.Shop.ShopId == shopId), cancellationToken);

    public Task<IReadOnlyList<CategoryCount>> GetCategoryCountsAsync(CancellationToken cancellationToken = default) =>
        CountCategoriesAsync(_db.Products, cancellationToken);

    private static async Task<IReadOnlyList<CategoryCount>> CountCategoriesAsync(IQueryable<Product> products, CancellationToken cancellationToken)
    {
        var counts = await products.AsNoTracking()
            .GroupBy(p => p.Category)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Name, x => x.Count, cancellationToken);

        return StorefrontCategories.All
            .Select(c => new CategoryCount(c, counts.GetValueOrDefault(c.Name)))
            .ToList();
    }

    public async Task<StoreStats> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        var delivered = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
        var productCount = await _db.Products.CountAsync(cancellationToken);
        var reviews = await _db.ProductReviews.AsNoTracking().Select(r => r.Rating).ToListAsync(cancellationToken);
        return new StoreStats(delivered, productCount, reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => (double)r), 1), reviews.Count);
    }

    private async Task<List<ProductCard>> ToCardsAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken)
    {
        var now = _clock.GetUtcNow().UtcDateTime;
        var productIds = products.Select(p => p.Id).ToArray();
        var flashSales = await _db.ActiveFlashSales(now).AsNoTracking()
            .Where(s => productIds.Contains(s.ProductId)).ToDictionaryAsync(s => s.ProductId, cancellationToken);
        var sales = await _db.OrderItems.AsNoTracking()
            .Where(i => i.Order!.Status != OrderStatus.Cancelled)
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Sold = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Sold, cancellationToken);

        var topSold = sales.Count == 0 ? 0 : sales.Values.Max();
        var newSince = _clock.GetUtcNow().UtcDateTime - NewProductWindow;

        return products.Select(p =>
        {
            var sold = sales.GetValueOrDefault(p.Id);
            var badge = topSold > 0 && sold == topSold ? ProductBadge.BestSeller
                : p.OriginalPrice > p.Price ? ProductBadge.Sale
                : p.CreatedAt >= newSince ? ProductBadge.New
                : ProductBadge.None;

            var flash = flashSales.GetValueOrDefault(p.Id);
            if (p.Stock <= 0 || flash?.Price >= p.Price) flash = null;
            return new ProductCard(p.Id, p.Name, StorefrontCategories.FindByName(p.Category), flash?.Price ?? p.Price,
                flash is null ? p.OriginalPrice : p.Price, p.Stock, p.ImageUrl, p.Rating, p.ReviewCount, sold,
                flash is null ? badge : ProductBadge.None, p.Shop?.Shop.Name, p.Shop?.ShopId,
                flash is null ? null : new DateTimeOffset(flash.EndsAt, TimeSpan.Zero));
        }).ToList();
    }
}
