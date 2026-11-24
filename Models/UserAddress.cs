namespace WebApplication1.Models;

/// <summary>ที่อยู่จัดส่งที่บันทึกในตาราง user_addresses ผู้ใช้แต่ละคนตั้งที่อยู่หลักได้หนึ่งรายการ</summary>
public class UserAddress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Label { get; set; } = string.Empty;

    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    public SavedAddressRegion? Region { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
