using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    /// <summary>
    /// โมเดลข้อมูลสำหรับการสมัครสมาชิกใหม่ (Register)
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// ชื่อบัญชีผู้ใช้ (Username) ความยาว 3-30 ตัวอักษร
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกชื่อผู้ใช้ (Username)")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Username ต้องมีความยาว 3-30 ตัวอักษร")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username ใช้ได้เฉพาะตัวอักษรภาษาอังกฤษ ตัวเลข และ _")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อ-นามสกุลจริง
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกชื่อ-นามสกุล")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// อีเมลของผู้ใช้งาน
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกอีเมล")]
        [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// เบอร์โทรศัพท์สำหรับการติดต่อ
        /// </summary>
        [Display(Name = "เบอร์โทรศัพท์")]
        [StringLength(20, ErrorMessage = "เบอร์โทรศัพท์ไม่เกิน 20 ตัวอักษร")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// รหัสผ่านสำหรับเข้าสู่ระบบ (ความยาวอย่างน้อย 4 ตัวอักษร)
        /// </summary>
        [Required(ErrorMessage = "กรุณากำหนดรหัสผ่าน")]
        [MinLength(4, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 4 ตัวอักษร")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// ยืนยันรหัสผ่านอีกครั้ง (ต้องตรงกับ Password)
        /// </summary>
        [Required(ErrorMessage = "กรุณายืนยันรหัสผ่าน")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "รหัสผ่านและการยืนยันรหัสผ่านไม่ตรงกัน")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
