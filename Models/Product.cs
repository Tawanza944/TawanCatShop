using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// โมเดลข้อมูลสินค้าจริง (ตาราง products ใน MySQL ฐานข้อมูล csi402db)
    /// รองรับการเพิ่มสินค้า จัดการสต็อก และขายสินค้าจริง
    /// </summary>
    [Table("products")]
    public class Product
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        public ProductShop? Shop { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อสินค้า")]
        [StringLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกหมวดหมู่")]
        [StringLength(100)]
        [Column("category")]
        public string Category { get; set; } = "อาหารสัตว์เลี้ยง";

        [Required(ErrorMessage = "กรุณากรอกราคา")]
        [Column("price")]
        public decimal Price { get; set; }

        [Column("original_price")]
        public decimal? OriginalPrice { get; set; }

        [Required(ErrorMessage = "กรุณากรอกจำนวนสต็อก")]
        [Column("stock")]
        public int Stock { get; set; } = 0;

        [Column("description")]
        public string? Description { get; set; }

        [Column("image_url")]
        public string? ImageUrl { get; set; }

        [Column("icon")]
        [StringLength(50)]
        public string Icon { get; set; } = "bi-box-seam";

        [Column("badge")]
        [StringLength(50)]
        public string? Badge { get; set; }

        [Column("rating")]
        public double Rating { get; set; } = 5.0;

        [Column("review_count")]
        public int ReviewCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}

