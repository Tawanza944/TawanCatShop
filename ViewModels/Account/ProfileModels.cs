using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using WebApplication1.ViewModels.Dashboard;

namespace WebApplication1.ViewModels.Account;

public sealed class ProfileForm
{
    [Required(ErrorMessage = "กรอกชื่อ-นามสกุล")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "ชื่อยาว 2–100 ตัวอักษร")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรอกชื่อผู้ใช้")]
    [RegularExpression(@"^[A-Za-z0-9_.]{3,50}$", ErrorMessage = "ชื่อผู้ใช้ใช้ได้เฉพาะ a-z, 0-9, _ และ . ยาว 3–50 ตัว")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรอกอีเมล")]
    [EmailAddress(ErrorMessage = "รูปแบบอีเมลไม่ถูกต้อง")]
    public string Email { get; set; } = string.Empty;

    [RegularExpression(@"^0\d{9}$", ErrorMessage = "เบอร์มือถือต้องมี 10 หลักและขึ้นต้นด้วย 0")]
    public string? PhoneNumber { get; set; }
}

public sealed class PasswordChangeForm
{
    [Required(ErrorMessage = "กรอกรหัสผ่านปัจจุบัน")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "กรอกรหัสผ่านใหม่")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "รหัสผ่านใหม่อย่างน้อย 6 ตัว")]
    public string NewPassword { get; set; } = string.Empty;

    [Compare(nameof(NewPassword), ErrorMessage = "ยืนยันรหัสผ่านไม่ตรงกัน")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class AddressForm
{
    public int Id { get; set; }

    public string Label { get; set; } = string.Empty;

    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public string ProvinceCode { get; set; } = string.Empty;
    public string DistrictCode { get; set; } = string.Empty;
    public string SubdistrictCode { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public UserAddress ToEntity() => new()
    {
        Id = Id, Label = Label, RecipientName = RecipientName, Phone = Phone, AddressLine = AddressLine,
        Region = new SavedAddressRegion
        {
            ProvinceCode = ProvinceCode, DistrictCode = DistrictCode, SubdistrictCode = SubdistrictCode, PostalCode = PostalCode
        }
    };
}

public sealed class ProfilePageViewModel
{
    public required string Tab { get; init; }

    public required User User { get; init; }

    public required DashboardSidebar Sidebar { get; init; }

    public required IReadOnlyList<UserAddress> Addresses { get; init; }

    public required IReadOnlyList<Order> RecentOrders { get; init; }

    public int OrderCount { get; init; }

    public ProfileForm Profile { get; init; } = new();
}
