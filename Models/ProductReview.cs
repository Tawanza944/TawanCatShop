namespace WebApplication1.Models;

/// <summary>รีวิวสินค้าที่ลูกค้าได้รับแล้ว บันทึกใน product_reviews หนึ่งรายการต่อผู้ใช้ต่อสินค้า</summary>
public class ProductReview
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int OrderId { get; set; }

    public byte Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
}
