using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels.Backoffice;

/// <summary>ฟอร์มแอดมินสำหรับเพิ่มหรือแก้ผู้ใช้ ใช้ Id เท่ากับ 0 และรหัสผ่านเมื่อสร้างบัญชี</summary>
public sealed class UserForm
{
    public int Id { get; set; }

    [Required(ErrorMessage = "กรอกชื่อผู้ใช้")]
    [RegularExpression(@"^[A-Za-z0-9_.]{3,50}$", ErrorMessage = "ชื่อผู้ใช้ใช้ได้เฉพาะ a-z, 0-9, _ และ . ยาว 3–50 ตัว")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรอกชื่อ-นามสกุล")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรอกอีเมล")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^0\d{8,9}$", ErrorMessage = "เบอร์โทรต้องขึ้นต้นด้วย 0 และมี 9–10 หลัก")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "เลือกบทบาท")]
    public string Role { get; set; } = "User";

    [StringLength(100, MinimumLength = 6, ErrorMessage = "รหัสผ่านอย่างน้อย 6 ตัว")]
    public string? Password { get; set; }
}
