using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    /// <summary>
    /// โมเดลข้อมูลคูปองและโปรโมชั่นส่วนลด (ตาราง coupons ใน MySQL)
    /// ผู้ดูแลระบบ (Admin) สามารถ เพิ่ม/แก้ไข/ลบ/เปิด-ปิด การใช้งานได้
    /// </summary>
    public class Coupon
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "กรุณากรอกรหัสโปรโมชั่น")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกชื่อโปรโมชั่น")]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// ประเภทส่วนลด: "Fixed" (ลดเป็นบาท), "Percent" (ลดเป็นเปอร์เซ็นต์ %), "FreeShipping" (ฟรีค่าจัดส่ง)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string DiscountType { get; set; } = "Fixed";

        /// <summary>
        /// มูลค่าส่วนลด (เช่น 50 บาท, หรือ 15%)
        /// </summary>
        public decimal DiscountValue { get; set; }

        /// <summary>
        /// ยอดสั่งซื้อขั้นต่ำในการใช้โค้ด (เช่น 399 บาท)
        /// </summary>
        public decimal MinSpend { get; set; } = 0;

        /// <summary>
        /// ส่วนลดสูงสุด (สำหรับกรณี Percent)
        /// </summary>
        public decimal? MaxDiscount { get; set; }

        /// <summary>
        /// สถานะเปิด/ปิดการใช้งาน (true = ใช้งานได้, false = ปิดใช้งาน)
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// หมวดสินค้าที่ใช้โค้ดได้ คั่นด้วยจุลภาค (null = ใช้ได้ทั้งตะกร้า)
        /// </summary>
        [StringLength(500)]
        public string? ApplicableCategories { get; set; }

        public IReadOnlyList<string> ApplicableCategoryList =>
            string.IsNullOrWhiteSpace(ApplicableCategories)
                ? []
                : ApplicableCategories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        /// <summary>
        /// วันหมดอายุ (ถ้ามี)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// วันที่สร้าง
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
