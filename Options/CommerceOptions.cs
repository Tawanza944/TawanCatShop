namespace WebApplication1.Options;

/// <summary>Business rules that may change without touching code. Bound from appsettings.json → "Commerce".</summary>
public sealed class CommerceOptions
{
    public const string SectionName = "Commerce";

    public decimal FreeShippingThreshold { get; set; } = 500m;

    public decimal ShippingFee { get; set; } = 40m;

    public decimal CommissionRate { get; set; } = 0.15m;

    public decimal RiderFeePerDelivery { get; set; } = 40m;

    public int MaxQuantityPerItem { get; set; } = 20;

    public int LowStockThreshold { get; set; } = 10;

    public string OrderNumberPrefix { get; set; } = "CAT";

    public string TimeZoneId { get; set; } = "Asia/Bangkok";
}
