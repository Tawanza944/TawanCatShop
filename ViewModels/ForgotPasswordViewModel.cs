using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    /// <summary>
    /// โมเดลข้อมูลสำหรับการรีเซ็ตหรือตั้งค่ารหัสผ่านใหม่เมื่อลืมรหัสผ่าน (Forgot Password)
    /// </summary>
    public class ForgotPasswordViewModel
    {
        /// <summary>
        /// อีเมลหรือชื่อผู้ใช้ของบัญชีที่ต้องการรีเซ็ตรหัสผ่าน
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกอีเมลหรือชื่อผู้ใช้ของคุณ")]
        [Display(Name = "อีเมล หรือ ชื่อผู้ใช้")]
        public string EmailOrUser { get; set; } = string.Empty;

        /// <summary>
        /// รหัสผ่านใหม่ที่ต้องการกำหนด
        /// </summary>
        [Required(ErrorMessage = "กรุณากรอกรหัสผ่านใหม่")]
        [MinLength(4, ErrorMessage = "รหัสผ่านต้องมีความยาวอย่างน้อย 4 ตัวอักษร")]
        [DataType(DataType.Password)]
        [Display(Name = "รหัสผ่านใหม่")]
        public string NewPassword { get; set; } = string.Empty;

        /// <summary>
        /// ยืนยันรหัสผ่านใหม่อีกครั้ง
        /// </summary>
        [Required(ErrorMessage = "กรุณายืนยันรหัสผ่านใหม่")]
        [DataType(DataType.Password)]
        [Display(Name = "ยืนยันรหัสผ่านใหม่")]
        [Compare("NewPassword", ErrorMessage = "รหัสผ่านใหม่และยืนยันรหัสผ่านไม่ตรงกัน")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
