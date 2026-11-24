using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Backoffice;

/// <summary>ข้อมูลฟอร์มแอดมินสำหรับเพิ่มและแก้คูปอง โดย Id เท่ากับ 0 หมายถึงเพิ่มใหม่</summary>
public sealed class CouponForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "กรอกโค้ด")]
    [RegularExpression(@"^[A-Za-z0-9]{3,50}$", ErrorMessage = "โค้ดใช้ได้เฉพาะตัวอักษรอังกฤษและตัวเลข 3–50 ตัว")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "ตั้งชื่อโปรโมชั่น")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public string DiscountType { get; set; } = "Fixed";

    [Range(0, 100_000)]
    public decimal DiscountValue { get; set; }

    [Range(0, 1_000_000)]
    public decimal MinSpend { get; set; }

    [Range(0, 100_000)]
    public decimal? MaxDiscount { get; set; }

    public List<string> Categories { get; set; } = [];

    public DateTime? ExpiresAt { get; set; }

    public bool IsActive { get; set; } = true;
}
