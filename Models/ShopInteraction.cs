namespace WebApplication1.Models;

public sealed class ShopReviewReply
{
    public int ReviewId { get; set; }
    public int ShopId { get; set; }
    public string Message { get; set; } = "";
}

public sealed class ShopFollow
{
    public int ShopId { get; set; }
    public int UserId { get; set; }
}

public sealed class ShopMessage
{
    public int Id { get; set; }
    public int ShopId { get; set; }
    public int UserId { get; set; }
    public bool FromShop { get; set; }
    public string Message { get; set; } = "";
    public DateTime SentAt { get; set; }
}
