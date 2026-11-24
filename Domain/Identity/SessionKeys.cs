namespace WebApplication1.Domain.Identity;

/// <summary>
/// ชื่อคีย์ในเซสชันที่ใช้ร่วมกันระหว่างตัวบันทึกและตัวอ่านข้อมูลผู้ใช้ที่ล็อกอิน
/// </summary>
public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string UserName = "UserName";
    public const string UserEmail = "UserEmail";
    public const string FullName = "FullName";
    public const string UserRole = "UserRole";
    public const string UserAvatar = "UserAvatar";
}
