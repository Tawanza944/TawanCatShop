namespace WebApplication1.Domain.Identity;

/// <summary>
/// ชื่อบทบาทตามที่บันทึกในตาราง <c>roles</c> ใช้ค่าคงที่นี้เมื่อตรวจหรือกำหนดบทบาท
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Raider = "Raider";
    public const string Owner = "Owner";
    public const string Accounting = "Accounting";
    public const string Shop = "Shop";

    public static IReadOnlyList<string> All { get; } = [Admin, User, Raider, Owner, Accounting, Shop];

    public static bool AreEqual(string? left, string? right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
}
