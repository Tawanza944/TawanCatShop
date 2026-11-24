using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// โมเดลข้อมูลผู้ใช้งาน (ตาราง users ใน MySQL)
    /// จัดเก็บข้อมูลบัญชีผู้ใช้ รหัสผ่านที่เข้ารหัส และข้อมูลการติดต่อ
    /// </summary>
    [Table("users")]
    public class User
    {
        /// <summary>
        /// รหัสประจำตัวผู้ใช้งาน (Primary Key อัตโนมัติ Auto-Increment)
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ชื่อ-นามสกุลจริงของผู้ใช้งาน
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อบัญชีผู้ใช้สำหรับเข้าสู่ระบบ (Username)
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอก Username")]
        [Column("username")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// ค่าแฮชรหัสผ่านที่ผ่านการเข้ารหัส SHA-256 ผสม Salt เพื่อความปลอดภัย
        /// </summary>
        [Required]
        [Column("password_hash")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// อีเมลของผู้ใช้งาน (ใช้สำหรับเข้าสู่ระบบและติดต่อสื่อสาร)
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอก Email")]
        [EmailAddress(ErrorMessage = "รูปแบบ Email ไม่ถูกต้อง")]
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// เบอร์โทรศัพท์สำหรับติดต่อหรือจัดส่งสินค้า
        /// </summary>
        [Column("phone_number")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// รูปโปรไฟล์ (/uploads/avatars/...) เก็บถาวรในฐานข้อมูล
        /// </summary>
        [Column("avatar_url")]
        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// วันที่และเวลาที่สร้างบัญชี
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// บทบาทการใช้งานของผู้ใช้ (ดึงมาจากตาราง staff ไม่ได้บันทึกลงตาราง users โดยตรง)
        /// </summary>
        [NotMapped]
        public string Role { get; set; } = "User";
    }

    /// <summary>
    /// โมเดลข้อมูลสิทธิ์หรือบทบาทผู้ใช้งาน (ตาราง roles ใน MySQL)
    /// </summary>
    [Table("roles")]
    public class Role
    {
        /// <summary>
        /// รหัสสิทธิ์ (Primary Key) เช่น 1=Admin, 2=User, 3=Raider, 4=Owner, 5=Accounting, 6=Shop
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// ชื่อสิทธิ์การใช้งาน
        /// </summary>
        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// โมเดลตารางเชื่อมโยงสิทธิ์การใช้งานของผู้ใช้ (ตาราง staff ใน MySQL)
    /// </summary>
    [Table("staff")]
    public class Staff
    {
        /// <summary>
        /// รหัสรายการ (Primary Key)
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// รหัสอ้างอิงผู้ใช้งาน (Foreign Key -> users.id)
        /// </summary>
        [Column("user_id")]
        public int UserId { get; set; }

        /// <summary>
        /// รหัสอ้างอิงบทบาท (Foreign Key -> roles.id)
        /// </summary>
        [Column("role_id")]
        public int RoleId { get; set; }

        /// <summary>
        /// สถานะการใช้งาน (เช่น active, inactive)
        /// </summary>
        [Column("status")]
        public string Status { get; set; } = "active";

        /// <summary>
        /// วันที่และเวลาที่กำหนดบทบาท
        /// </summary>
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
