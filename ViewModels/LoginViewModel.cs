using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    /// <summary>
    /// โมเดลข้อมูลสำหรับการเข้าสู่ระบบ (Login)
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// อีเมลหรือชื่อผู้ใช้ที่ใช้เข้าสู่ระบบ
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกอีเมลหรือชื่อผู้ใช้")]
        [Display(Name = "อีเมลหรือชื่อผู้ใช้")]
        public string EmailOrUser { get; set; } = string.Empty;

        /// <summary>
        /// รหัสผ่านสำหรับเข้าสู่ระบบ
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่าน")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// หน้าภายในเว็บที่จะกลับไปหลังล็อกอิน เมื่อผู้ใช้ถูกส่งมาจากหน้าที่ต้องมีสิทธิ์
        /// </summary>
        public string? ReturnUrl { get; set; }
    }
}
