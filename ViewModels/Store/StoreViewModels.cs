using WebApplication1.Domain.Catalog;
using WebApplication1.Models;

namespace WebApplication1.ViewModels.Store;

public enum ProductBadge
{
    None,
    BestSeller,
    New,
    Sale
}

/// <summary>A product as shown on a storefront card.</summary>
public sealed record ProductCard(
    int Id,
    string Name,
    StorefrontCategory? Category,
    decimal Price,
    decimal? OriginalPrice,
    int Stock,
    string? ImageUrl,
    double Rating,
    int ReviewCount,
    int Sold,
    ProductBadge Badge,
    string? ShopName = null,
    int? ShopId = null,
    DateTimeOffset? FlashEndsAt = null)
{
    public bool OnSale => OriginalPrice is { } original && original > Price;

    public bool InStock => Stock > 0;

    public string BadgeLabel => Badge switch
    {
        ProductBadge.BestSeller => "ขายดี",
        ProductBadge.New => "ของใหม่",
        ProductBadge.Sale => OriginalPrice is { } original && original > 0 ? $"ลด {Math.Round((1 - Price / original) * 100):0}%" : "ลดราคา",
        _ => string.Empty
    };
}

public enum ProductSort
{
    Recommended,
    PriceLow,
    PriceHigh,
    BestSelling,
    Newest
}

/// <summary>Filters from the product list query string.</summary>
public sealed record ProductQuery(string? CategoryKey, string? Search, ProductSort Sort, decimal? MinPrice, decimal? MaxPrice, int? ShopId = null);

public sealed record StorefrontShop(int Id, string Name, string? ImageUrl, int Followers, double? Rating);

public sealed record CategoryCount(StorefrontCategory Category, int Count);

public sealed record StoreStats(int DeliveredOrders, int ProductCount, double AverageRating, int ReviewCount);

public sealed class HomePageViewModel
{
    public required StoreStats Stats { get; init; }

    public required IReadOnlyList<CategoryCount> Categories { get; init; }

    public required IReadOnlyList<ProductCard> BestSellers { get; init; }

    public required IReadOnlyList<ProductCard> CatPicks { get; init; }

    public required IReadOnlyList<ProductCard> DogPicks { get; init; }

    public required IReadOnlyList<ProductCard> Deals { get; init; }
    public IReadOnlyList<ProductCard> FlashSales { get; init; } = [];
}

public sealed class ProductListViewModel
{
    public StorefrontShop? Shop { get; init; }
    public bool IsFollowingShop { get; init; }
    public bool CanInteractWithShop { get; init; }
    public bool IsSignedIn { get; init; }

    public required ProductQuery Query { get; init; }

    public required IReadOnlyList<ProductCard> Products { get; init; }

    public required IReadOnlyList<CategoryCount> Categories { get; init; }

    public int TotalProducts { get; init; }

    public StorefrontCategory? SelectedCategory => StorefrontCategories.FindByKey(Query.CategoryKey);
}

public sealed record ReviewItem(int Id, string ReviewerName, int Rating, string? Comment, DateTime CreatedAt, bool IsMine, string? ShopReply = null);

/// <summary>Whether the signed-in user can review this product, and their existing review if any.</summary>
public sealed record ReviewEligibility(bool CanReview, string? Reason, ReviewItem? ExistingReview);

public sealed class ProductDetailViewModel
{
    public required ProductCard Card { get; init; }

    public string? Description { get; init; }

    public required IReadOnlyList<ReviewItem> Reviews { get; init; }

    public required IReadOnlyList<int> RatingBreakdown { get; init; }

    public required ReviewEligibility Eligibility { get; init; }

    public required IReadOnlyList<ProductCard> Related { get; init; }
}

public sealed class PromotionPageViewModel
{
    public required IReadOnlyList<Coupon> Coupons { get; init; }

    public required IReadOnlySet<int> SavedCouponIds { get; init; }

    public required IReadOnlySet<string> UsedCodes { get; init; }

    public required IReadOnlyList<ProductCard> Deals { get; init; }
}

/// <summary>One coupon drawn as a ticket (promotion page and "my coupons"). <see cref="Used"/> shows the "ใช้แล้ว" stamp.</summary>
public sealed record CouponTicket(Coupon Coupon, bool Used, bool UseToShop = false, bool Saved = false);
