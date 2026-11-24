using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// โมเดลบันทึกประวัติการใช้งานโค้ดส่วนลดของสมาชิก (ตาราง user_coupons ใน MySQL)
    /// จัดเก็บประวัติเพื่อจำกัดสิทธิ์: 1 โค้ด สามารถใช้ได้ 1 ครั้ง ต่อ 1 บัญชีผู้ใช้เท่านั้น
    /// </summary>
    public class UserCoupon
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// รหัสสมาชิกที่ใช้โค้ด (เชื่อมกับ users.id)
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// รหัสโค้ดส่วนลด เช่น LOVE50, FREESHIP, CATSNACK15
        /// </summary>
        [Required]
        [StringLength(50)]
        public string CouponCode { get; set; } = string.Empty;

        /// <summary>
        /// รหัสคำสั่งซื้อที่นำโค้ดนี้ไปใช้ เช่น CAT-20261118-1234
        /// </summary>
        [StringLength(64)]
        public string? OrderNo { get; set; }

        /// <summary>
        /// จำนวนเงินส่วนลดที่ได้รับ (บาท)
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// เวลาที่ใช้โค้ดส่วนลด
        /// </summary>
        public DateTime UsedAt { get; set; } = DateTime.UtcNow;
    }
}
