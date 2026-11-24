using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Backoffice;

/// <summary>ฟอร์มเพิ่มหรือแก้สินค้าที่ส่งจากหลังร้าน ค่า Id เท่ากับ 0 หมายถึงเพิ่มสินค้าใหม่</summary>
public sealed class ProductForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "กรอกชื่อสินค้า")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "ชื่อสินค้ายาว 3–255 ตัวอักษร")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "เลือกหมวดสินค้า")]
    public string Category { get; set; } = string.Empty;

    [Range(1, 1_000_000, ErrorMessage = "ราคาขายต้องมากกว่า 0")]
    public decimal Price { get; set; }

    [Range(0, 1_000_000)]
    public decimal? OriginalPrice { get; set; }

    [Range(0, 100_000, ErrorMessage = "สต็อกต้องเป็น 0 ขึ้นไป")]
    public int Stock { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Url(ErrorMessage = "ลิงก์รูปไม่ถูกต้อง")]
    public string? ImageUrl { get; set; }

    public IFormFile? ImageFile { get; set; }
}
