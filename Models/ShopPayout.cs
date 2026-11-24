namespace WebApplication1.Models;

public sealed class ShopPayout
{
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public decimal Commission { get; set; }
    public decimal NetAmount { get; set; }
    public int ApprovedBy { get; set; }
    public DateTime ApprovedAt { get; set; }
    public int? RecordedBy { get; set; }
    public DateTime? RecordedAt { get; set; }
    public DateOnly? TransferDate { get; set; }
    public string? TransferReference { get; set; }
}
