namespace WebApplication1.Domain.Catalog;

/// <summary>A storefront category: URL key, the exact <c>products.category</c> value, and the label shown to shoppers.</summary>
public sealed record StorefrontCategory(string Key, string Name, string Label, string Icon, bool ForDogs = false);

/// <summary>
/// The six categories the shop sells. <c>products.category</c> must hold one of the <see cref="StorefrontCategory.Name"/> values.
/// </summary>
public static class StorefrontCategories
{
    public static readonly StorefrontCategory CatFood = new("food", "อาหารแมว", "อาหารแมว", "bi-basket");
    public static readonly StorefrontCategory CatTreats = new("treats", "ขนมแมว", "ขนมแมว", "bi-cookie");
    public static readonly StorefrontCategory Litter = new("litter", "ทรายแมว", "ทรายแมว", "bi-box");
    public static readonly StorefrontCategory Toys = new("toys", "ของเล่นแมว", "ของเล่น", "bi-balloon");
    public static readonly StorefrontCategory Beds = new("condo", "คอนโด & ที่นอน", "ที่นอนและคอนโด", "bi-house-door");
    public static readonly StorefrontCategory Dog = new("dog", "อาหาร & ขนมสุนัข", "ของน้องหมา", "bi-heart", ForDogs: true);

    public static IReadOnlyList<StorefrontCategory> All { get; } = [CatFood, CatTreats, Litter, Toys, Beds, Dog];

    public static StorefrontCategory? FindByKey(string? key) =>
        All.FirstOrDefault(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase));

    public static StorefrontCategory? FindByName(string? name) =>
        All.FirstOrDefault(c => string.Equals(c.Name, name?.Trim(), StringComparison.Ordinal));

    public static IReadOnlyList<string> Names { get; } = All.Select(c => c.Name).ToArray();
}
