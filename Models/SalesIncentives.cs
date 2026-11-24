namespace WebApplication1.Models;

public sealed class FlashReservation
{
    public int OrderItemId { get; set; }
    public int FlashSaleId { get; set; }
}

public sealed class FlashSale
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Price { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int Quota { get; set; }
    public int Sold { get; set; }
}

public sealed class OrderIncentives
{
    public int OrderId { get; set; }
    public decimal FlashDiscount { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal RewardUsed { get; set; }
    public decimal EligibleSpend { get; set; }
}
