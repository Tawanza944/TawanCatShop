namespace WebApplication1.Models;

public sealed class SavedCoupon
{
    public int UserId { get; set; }
    public int CouponId { get; set; }
    public DateTime SavedAt { get; set; }
}
